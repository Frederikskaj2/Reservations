using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Diagnostics;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.SettleReservation;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class SettleReservationShell
{
    public static EitherAsync<Failure<Unit>, OrderDetails> SettleReservation(
        IOrdersEmailService emailService,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        SettleReservationCommand command,
        CancellationToken cancellationToken) =>
        from orderEntity in ReadResidentOrderEntity(reader, command.OrderId, cancellationToken)
        from userEntity in reader.ReadWithETag<User>(orderEntity.Value.UserId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        from output in SettleReservationCore(options, timeConverter, new(command, userEntity.Value, orderEntity.Value, transactionId)).ToAsync()
        from _1 in Write(writer, userEntity, orderEntity, output, cancellationToken)
        from _2 in SendReservationSettledEmail(emailService, command, output.User, output.Reservation, cancellationToken)
        from _3 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        from userFullNamesMap in ReadUserFullNamesMapForOrder(reader, output.Order, cancellationToken)
        select new OrderDetails(output.Order, output.User, userFullNamesMap);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<User> userEntity,
        ETaggedEntity<Order> orderEntity,
        SettleReservationOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntity).Add(orderEntity),
                tracker => tracker.Update(output.User).Update(output.Order).Add(output.Transaction),
                cancellationToken)
            .MapWriteError();

    static EitherAsync<Failure<Unit>, Unit> SendReservationSettledEmail(
        IOrdersEmailService ordersEmailService, SettleReservationCommand command, User user, Reservation reservation, CancellationToken cancellationToken) =>
        ordersEmailService.Send(
            new ReservationSettledEmailModel(
                user.Email(),
                user.FullName,
                command.OrderId,
                new(reservation.ResourceId, reservation.Extent.Date),
                reservation.Price.Case switch
                {
                    Price price => price.Deposit,
                    _ => throw new UnreachableException(),
                },
                command.Damages,
                command.Description.ToNullableReference()),
            cancellationToken);

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;
}
