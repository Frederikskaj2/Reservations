using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

abstract class BackgroundService : IHostedService, IDisposable
{
    readonly CancellationTokenSource cancellationTokenSource = new();
    Task? executingTask;

    public virtual void Dispose()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        executingTask = ExecuteAsync(cancellationTokenSource.Token);
        return executingTask.IsCompleted ? executingTask : Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (executingTask is null)
            return;

        try
        {
            cancellationTokenSource.Cancel();
        }
        finally
        {
            await Task.WhenAny(executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}