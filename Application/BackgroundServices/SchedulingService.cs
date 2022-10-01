using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

sealed class SchedulingService<TService> : IHostedService, IDisposable where TService : IScheduledService
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly ILogger logger;
    readonly IServiceProvider serviceProvider;
    Timer? timer;

    public SchedulingService(ILogger<SchedulingService<TService>> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        cancellationTokenSource = new();
    }

    public void Dispose()
    {
        timer?.Dispose();
        cancellationTokenSource.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting scheduling service");
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        logger.LogInformation("Creating periodic timer with start delay {StartDelay} and interval {Interval}", service.StartDelay, service.Interval);
        timer = new Timer(DoWork, null, service.StartDelay.ToTimeSpan(), service.Interval.ToTimeSpan());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping scheduling service");
        timer?.Change(Timeout.Infinite, 0);
        cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    void DoWork(object? state)
    {
        try
        {
            logger.LogInformation("Scheduling service is doing work");
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            service.DoWork(cancellationTokenSource.Token).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred");
        }
    }
}
