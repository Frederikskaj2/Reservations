using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Cleaning;

public interface ICleaningEmailService
{
    Task<Unit> Send(CleaningScheduleEmail model, Seq<EmailUser> users, CancellationToken cancellationToken);
}
