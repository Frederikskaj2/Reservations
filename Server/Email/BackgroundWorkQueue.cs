using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class BackgroundWorkQueue<TService> : IBackgroundWorkQueue<TService>
    {
        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<Func<TService, CancellationToken, Task>> queue = new ConcurrentQueue<Func<TService, CancellationToken, Task>>();

        public void Enqueue(Func<TService, CancellationToken, Task> asyncAction)
        {
            if (asyncAction is null)
                throw new ArgumentNullException(nameof(asyncAction));

            queue.Enqueue(asyncAction);
            signal.Release();
        }

        public async Task<Func<TService, CancellationToken, Task>> Dequeue(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            return queue.TryDequeue(out var asyncAction) ? asyncAction : (_, __) => Task.CompletedTask;
        }
    }
}