using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class BackgroundWorkerService<TService> : BackgroundService
    {
        private readonly IBackgroundWorkQueue<TService> queue;
        private readonly IServiceProvider serviceProvider;

        public BackgroundWorkerService(IServiceProvider serviceProvider, IBackgroundWorkQueue<TService> queue)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var asyncAction = await queue.Dequeue(stoppingToken);
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    await asyncAction(service, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception)
                {
                }
            }
        }
    }
}