using System.Collections.Generic;
using System.Threading;

namespace Frederikskaj2.Reservations.Emails;

public interface IEmailDequeuer
{
    IAsyncEnumerable<Email> Dequeue(CancellationToken cancellationToken);
}
