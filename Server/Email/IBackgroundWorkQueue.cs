using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.Email
{
    public interface IBackgroundWorkQueue<TService>
    {
        void Enqueue(Func<TService, CancellationToken, Task> asyncAction);
        Task<Func<TService, CancellationToken, Task>> Dequeue(CancellationToken cancellationToken);
    }
}