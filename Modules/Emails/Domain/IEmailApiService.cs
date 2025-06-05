using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

public interface IEmailApiService
{
    ValueTask Send(EmailMessage message, CancellationToken cancellationToken);
}
