using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Bank;

public interface IBankEmailService
{
    Task<Unit> Send(DebtReminderEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(PayInEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(PayOutEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(PostingsForMonthEmailModel model, HashMap<UserId, string> userFullNames, CancellationToken cancellationToken);
}
