using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.RoomAccess;

public interface IRoomAccessEmailService
{
    Task<Unit> Send(RoomEntryCodeEmail model, CancellationToken cancellationToken);
}
