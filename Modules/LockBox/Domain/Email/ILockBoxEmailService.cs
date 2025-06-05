using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.LockBox;

public interface ILockBoxEmailService
{
    Task<Unit> Send(LockBoxCodesOverviewEmail model, CancellationToken cancellationToken);
}
