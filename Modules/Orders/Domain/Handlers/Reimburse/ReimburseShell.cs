using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.Reimburse;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class ReimburseShell
{
    public static EitherAsync<Failure<Unit>, Unit> Reimburse(
        IJobScheduler jobScheduler, IEntityReader reader, IEntityWriter writer, ReimburseCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        let output = ReimburseCore(new(command, userEntity.Value, transactionId))
        from _1 in Write(writer, userEntity, output, cancellationToken)
        from _2 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        select unit;

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer, ETaggedEntity<User> userEntity, ReimburseOutput output, CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntity),
                tracker => tracker.Update(output.User).Add(output.Transaction),
                cancellationToken)
            .MapWriteError();

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;
}
