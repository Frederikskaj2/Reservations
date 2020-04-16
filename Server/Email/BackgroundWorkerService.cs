using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class BackgroundWorkerService<TService> : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IBackgroundWorkQueue<TService> queue;
        private readonly IServiceProvider serviceProvider;

        public BackgroundWorkerService(
            ILogger<BackgroundWorkerService<TService>> logger, IServiceProvider serviceProvider,
            IBackgroundWorkQueue<TService> queue)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [SuppressMessage(
            "Design", "CA1031:Do not catch general exception types",
            Justification = "This method should not pass exceptions to callers.")]
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
                catch (Exception exception)
                {
                    logger.LogError(exception, "An error occurred");
                }
            }
        }
    }
}