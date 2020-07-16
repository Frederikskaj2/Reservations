using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.Email
{
    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The suffix is appropriate for this class.")]
    public interface IBackgroundWorkQueue<TService>
    {
        void Enqueue(Func<TService, CancellationToken, Task> asyncAction);
        Task<Func<TService, CancellationToken, Task>> Dequeue(CancellationToken cancellationToken);
    }
}