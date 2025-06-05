using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.SendCleaningScheduleUpdateShell;

namespace Frederikskaj2.Reservations.Cleaning;

class SendCleaningScheduleUpdateJob(IDateProvider dateProvider, ICleaningEmailService emailService, IEntityReader reader, IEntityWriter writer) : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendCleaningScheduleUpdate(emailService, reader, writer, new(dateProvider.Today), cancellationToken);
}
