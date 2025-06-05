using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

public interface IUsersEmailService
{
    Task<Unit> Send(ConfirmEmailEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(NewPasswordEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(UserDeletedEmailModel model, CancellationToken cancellationToken);
}
