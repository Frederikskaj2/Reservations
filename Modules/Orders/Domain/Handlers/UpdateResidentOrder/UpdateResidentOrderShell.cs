using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.UpdateResidentOrder;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Orders;

public static class UpdateResidentOrderShell
{
    public static EitherAsync<Failure<Unit>, OrderDetails> UpdateResidentOrder(
        IOrdersEmailService emailService,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        UpdateResidentOrderCommand command,
        CancellationToken cancellationToken) =>
        from orderEntity in ReadResidentOrderEntity(reader, command.OrderId, cancellationToken)
        from userEntity in reader.ReadWithETag<User>(orderEntity.Value.UserId, cancellationToken).MapReadError()
        from transactionIdOption in CreateTransactionIdIfNeeded(reader, writer, command.CancelledReservations, cancellationToken)
        let input = new UpdateResidentOrderInput(command, userEntity.Value, orderEntity.Value, transactionIdOption)
        from output in UpdateResidentOrderCore(options, timeConverter, input).ToAsync()
        from _1 in Write(writer, userEntity, orderEntity, output, cancellationToken)
        from _2 in SendReservationsCancelledEmail(
            emailService, command.CancelledReservations, output.User, output.Order, output.TransactionOption, cancellationToken)
        from _3 in SendNoFeeCancellationIsAllowed(
            emailService, options.CancellationWithoutFeeDuration, command.OrderId, output.User, output.IsCancellationWithoutFeeAllowed, cancellationToken)
        from _4 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        from _5 in UpdateCleaningSchedule(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        from userFullNamesMap in ReadUserFullNamesMapForOrder(reader, output.Order, cancellationToken)
        select new OrderDetails(output.Order, output.User, userFullNamesMap);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<User> userEntity,
        ETaggedEntity<Order> orderEntity,
        UpdateResidentOrderOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntity).Add(orderEntity),
                tracker => tracker.Update(output.User).Update(output.Order).TryAdd(output.TransactionOption),
                cancellationToken)
            .MapWriteError();

    static EitherAsync<Failure<Unit>, Option<TransactionId>> CreateTransactionIdIfNeeded(
        IEntityReader reader, IEntityWriter writer, HashSet<ReservationIndex> cancelledReservations, CancellationToken cancellationToken) =>
        !cancelledReservations.IsEmpty
            ? CreateTransactionId(reader, writer, cancellationToken)
            : Option<TransactionId>.None;

    static EitherAsync<Failure<Unit>, Option<TransactionId>> CreateTransactionId(
        IEntityReader reader, IEntityWriter writer, CancellationToken cancellationToken) =>
        from id in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        select Some(TransactionId.FromInt32(id));

    static EitherAsync<Failure<Unit>, Unit> SendNoFeeCancellationIsAllowed(
        IOrdersEmailService emailService,
        Duration duration,
        OrderId orderId,
        User user,
        bool isCancellationWithoutFeeAllowed,
        CancellationToken cancellationToken) =>
        isCancellationWithoutFeeAllowed
            ? emailService
                .Send(new NoFeeCancellationAllowedEmailModel(user.Email(), user.FullName, orderId, duration), cancellationToken)
                .ToRightAsync<Failure<Unit>, Unit>()
            : unit;

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;

    static Unit UpdateCleaningSchedule(IJobScheduler jobScheduler, User user) =>
        // The confirm orders job will queue the update cleaning schedule job, so this
        // is only necessary if that job wasn't queued.
        !user.IsOwedMoney() ? jobScheduler.Queue(JobName.UpdateCleaningSchedule) : unit;
}
