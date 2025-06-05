using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.UpdateCleaningScheduleShell;

namespace Frederikskaj2.Reservations.Cleaning;

class UpdateCleaningScheduleJob(
    IDateProvider dateProvider,
    IOptionsSnapshot<OrderingOptions> options,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        UpdateCleaningSchedule(options.Value, reader, writer, new(dateProvider.Today), cancellationToken);
}
