using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.UpdateLockBoxCodesShell;

namespace Frederikskaj2.Reservations.LockBox;

class UpdateLockBoxCodesJob(IDateProvider dateProvider, IEntityReader reader, IEntityWriter writer) : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        UpdateLockBoxCodes(reader, writer, new(dateProvider.Today), cancellationToken);
}
