using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.LockBox.UpdateLockBoxCodes;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

public static class UpdateLockBoxCodesShell
{
    public static EitherAsync<Failure<Unit>, Unit> UpdateLockBoxCodes(
        IEntityReader reader, IEntityWriter writer, UpdateLockBoxCodesCommand command, CancellationToken cancellationToken) =>
        from lockBoxCodesEntity in ReadLockBoxCodesEntity(reader, cancellationToken)
        let output = UpdateLockBoxCodesCore(new(command, lockBoxCodesEntity))
        from _ in Write(writer, lockBoxCodesEntity, output, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer, OptionalEntity<LockBoxCodes> lockBoxCodesEntity, UpdateLockBoxCodesOutput output, CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.TryAdd(lockBoxCodesEntity),
                tracker => tracker.AddOrUpdate(output.LockBoxCodesEntity),
                cancellationToken)
            .MapWriteError();
}
