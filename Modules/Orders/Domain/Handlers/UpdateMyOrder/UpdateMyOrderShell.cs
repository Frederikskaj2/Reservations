using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.OrdersLockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFactory;
using static Frederikskaj2.Reservations.Orders.UpdateMyOrder;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class UpdateMyOrderShell
{
    public static EitherAsync<Failure<Unit>, UpdateMyOrderResult> UpdateMyOrder(
        IOrdersEmailService emailService,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        UpdateMyOrderCommand command,
        CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from orderEntity in ReadResidentOrderEntity(reader, command.OrderId, cancellationToken)
        from lockBoxCodes in ReadLockBoxCodes(reader, cancellationToken)
        from transactionIdOption in CreateTransactionIdIfNeeded(reader, writer, command.CancelledReservations, cancellationToken)
        let input = new UpdateMyOrderInput(command, userEntity.Value, orderEntity.Value, transactionIdOption)
        from output in UpdateMyOrderCore(options, timeConverter, input).ToAsync()
        from _1 in Write(writer, userEntity, orderEntity, output, cancellationToken)
        from _2 in SendReservationsCancelledEmail(
            emailService, command.CancelledReservations, output.User, output.Order, output.TransactionOption, cancellationToken)
        from _3 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        from _4 in UpdateCleaningSchedule(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        let today = timeConverter.GetDate(command.Timestamp)
        let lockBoxCodesForOrder = CreateLockBoxCodesForOrder(options, today, output.Order, lockBoxCodes)
        select new UpdateMyOrderResult(CreateResidentOrder(options, today, output.Order, output.User, lockBoxCodesForOrder), output.IsUserDeletionConfirmed);

    static EitherAsync<Failure<Unit>, Option<TransactionId>> CreateTransactionIdIfNeeded(
        IEntityReader reader, IEntityWriter writer, HashSet<ReservationIndex> cancelledReservations, CancellationToken cancellationToken) =>
        !cancelledReservations.IsEmpty
            ? CreateTransactionId(reader, writer, cancellationToken)
            : Option<TransactionId>.None;

    static EitherAsync<Failure<Unit>, Option<TransactionId>> CreateTransactionId(
        IEntityReader reader, IEntityWriter writer, CancellationToken cancellationToken) =>
        from id in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        select Some(TransactionId.FromInt32(id));

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<User> userEntity,
        ETaggedEntity<Order> orderEntity,
        UpdateMyOrderOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntity).Add(orderEntity),
                tracker => tracker.Update(output.User).Update(output.Order).TryAdd(output.TransactionOption),
                cancellationToken)
            .MapWriteError();

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;

    static Unit UpdateCleaningSchedule(IJobScheduler jobScheduler, User user) =>
        // The confirm orders job will queue the update cleaning schedule job, so this
        // is only necessary if that job wasn't queued.
        !user.IsOwedMoney() ? jobScheduler.Queue(JobName.UpdateCleaningSchedule) : unit;
}
