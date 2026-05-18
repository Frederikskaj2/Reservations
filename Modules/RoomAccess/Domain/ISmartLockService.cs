using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.RoomAccess;

public interface ISmartLockService
{
    Task<ISmartLockAuthorizationContext> GetSmartLockAuthorizationContext(CancellationToken cancellationToken);
    Task<Unit> SynchronizeSmartLockAuthorizationSet(ISmartLockAuthorizationContext smartLockAuthorizationContext, CancellationToken cancellationToken);
}
