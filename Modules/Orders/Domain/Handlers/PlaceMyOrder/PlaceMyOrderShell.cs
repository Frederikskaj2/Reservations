using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.OrdersQueries;
using static Frederikskaj2.Reservations.Orders.PlaceMyOrder;
using static Frederikskaj2.Reservations.Orders.ReservationValidator;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFactory;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFunctions;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class PlaceMyOrderShell
{
    public static EitherAsync<Failure<Unit>, ResidentOrder> PlaceMyOrder(
        IOrdersEmailService emailService,
        IReadOnlySet<LocalDate> holidays,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        PlaceMyOrderCommand command,
        CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from _1 in ValidateResidentCanPlaceOrder(userEntity.Value)
        from allActiveOrderEntities in reader.QueryWithETag(GetAllActiveOrdersQuery, cancellationToken).MapReadError()
        let today = timeConverter.GetDate(command.Timestamp)
        from _2 in ValidateReservations(ValidateByResident(holidays, options), command.Reservations, GetActiveReservations(allActiveOrderEntities), today)
        from lockBoxCodesEntity in ReadLockBoxCodes(reader, cancellationToken)
        from subscribedEmailUsers in ReadSubscribedEmailUsers(reader, EmailSubscriptions.NewOrder, cancellationToken)
        from orderId in CreateId(reader, writer, nameof(Order), cancellationToken)
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        let input = new PlaceMyOrderInput(command, today, userEntity.Value, lockBoxCodesEntity, orderId, transactionId)
        let output = PlaceMyOrderCore(holidays, options, input)
        from _3 in Write(writer, userEntity, output, cancellationToken)
        from _4 in SendEmails(emailService, subscribedEmailUsers, output.User, output.Order, output.Payment, cancellationToken)
        from _5 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        select CreateResidentOrder(options, today, output.Order, output.User, output.LockBoxCodesForOrder);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer, ETaggedEntity<User> userEntity, PlaceMyOrderOutput output, CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntity),
                tracker => tracker.Update(output.User).Add(output.Order).Add(output.Transaction),
                cancellationToken)
            .MapWriteError();

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;
}
