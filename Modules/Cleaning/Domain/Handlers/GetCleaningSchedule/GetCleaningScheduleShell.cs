using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.CleaningFunctions;

namespace Frederikskaj2.Reservations.Cleaning;

public static class GetCleaningScheduleShell
{
    public static EitherAsync<Failure<Unit>, CleaningSchedule> GetCleaningSchedule(
        IEntityReader reader, IEntityWriter writer, CancellationToken cancellationToken) =>
        from entity in ReadCurrentCleaningScheduleEntity(reader, cancellationToken)
        from _ in writer.Write(collector => collector.TryAdd(entity), tracker => tracker.AddOrUpdate(entity), cancellationToken).MapWriteError()
        select entity.EntityValue;
}
