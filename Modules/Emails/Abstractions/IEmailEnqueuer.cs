using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

public interface IEmailEnqueuer
{
    ValueTask Enqueue(Email email, CancellationToken cancellationToken);
}
