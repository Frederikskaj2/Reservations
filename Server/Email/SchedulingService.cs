using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class SchedulingService
    {
        protected static readonly TimeSpan StartDelay = TimeSpan.FromMinutes(1);
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
    internal sealed class SchedulingService<TService> : SchedulingService, IHostedService, IDisposable where TService : IScheduledService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private Timer? timer;

        public SchedulingService(ILogger<SchedulingService<TService>> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Dispose() => timer?.Dispose();

        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting scheduling service");

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            timer = new Timer(DoWork, null, StartDelay, service.Interval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Stopping scheduling service");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        [SuppressMessage(
            "Design", "CA1031:Do not catch general exception types",
            Justification = "This method should not pass exceptions to callers.")]
        private void DoWork(object? state)
        {
            logger.LogInformation("Scheduling service is doing work");
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            try
            {
                service.DoWork().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred");
            }
        }
    }
}