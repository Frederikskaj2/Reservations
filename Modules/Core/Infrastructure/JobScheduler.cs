using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Core;

class JobScheduler : BackgroundService, IJobScheduler
{
    readonly IClock clock;
    readonly ILogger<JobScheduler> logger;
    readonly IOptions<JobSchedulerOptions> options;
    readonly IServiceProvider serviceProvider;
    readonly Dictionary<JobName, Job> schedule;
    readonly SemaphoreSlim gate = new(1, 1);
    CancellationTokenSource? cancellationTokenSource;

    public JobScheduler(
        IClock clock,
        ILogger<JobScheduler> logger,
        IOptions<JobSchedulerOptions> options,
        IEnumerable<IJobRegistration> registrations,
        IServiceProvider serviceProvider)
    {
        this.clock = clock;
        this.logger = logger;
        this.options = options;
        this.serviceProvider = serviceProvider;
        var now = clock.GetCurrentInstant();
        schedule = registrations.ToDictionary(
            registration => registration.Name,
            registration => new Job(
                registration,
                registration.Schedule.GetNextExecutionTime(now, isFirstExecution: true),
                IsImmediate: false));
        logger.LogDebug("Configured jobs are {Jobs}", schedule.Keys);
    }

    public override void Dispose()
    {
        cancellationTokenSource?.Dispose();
        gate.Dispose();
        base.Dispose();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is a main loop and should throw exceptions.")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.IsEnabled)
        {
            logger.LogInformation("Job scheduling is disabled");
            return;
        }

        if (schedule.Count is 0)
        {
            logger.LogWarning("No jobs are scheduled jobs");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextJob = GetNextJob(stoppingToken);
            logger.LogTrace("Next job is {JobName}", nextJob.Registration.Name);
            try
            {
                var now = clock.GetCurrentInstant();
                if (nextJob.NextExecutionTime > now)
                {
                    var delay = GetDelay(nextJob, now);
                    logger.LogTrace("Delaying {Delay} before executing job {JobName}", delay, nextJob.Registration.Name);
                    using var delayCancellationTokenSource = CreateDelayCancellationTokenSource(stoppingToken);
                    await Task.Delay(delay.ToTimeSpan(), delayCancellationTokenSource.Token);
                }
                else
                {
                    ClearDelayCancellationTokenSource(stoppingToken);
                    await ExecuteJob(nextJob, stoppingToken);
                    ScheduleNextExecutionOfJob(nextJob, stoppingToken);
                }
            }
            catch (OperationCanceledException exception)
            {
                if (exception.CancellationToken != stoppingToken)
                    try
                    {
                        await gate.WaitAsync(stoppingToken);
                        cancellationTokenSource?.Dispose();
                        cancellationTokenSource = null;
                    }
                    finally
                    {
                        if (gate.CurrentCount is 0)
                            gate.Release();
                    }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Job {JobName} failed", nextJob.Registration.Name);
                ScheduleNextExecutionOfJob(nextJob, stoppingToken);
            }
        }
    }

    Job GetNextJob(CancellationToken stoppingToken)
    {
        try
        {
            gate.Wait(stoppingToken);
            var jobs = schedule.Values.OrderByDescending(job => job.IsImmediate).ThenBy(job => job.NextExecutionTime);
            return jobs.First();
        }
        finally
        {
            if (gate.CurrentCount is 0)
                gate.Release();
        }
    }

    static Duration GetDelay(Job nextJob, Instant now)
    {
        var delay = nextJob.NextExecutionTime - now;
        return delay >= Duration.FromSeconds(1) ? delay : Duration.FromSeconds(1);
    }

    CancellationTokenSource CreateDelayCancellationTokenSource(CancellationToken stoppingToken)
    {
        try
        {
            gate.Wait(stoppingToken);
            var isAlreadyCancelled = cancellationTokenSource?.IsCancellationRequested ?? false;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new();
            if (isAlreadyCancelled)
                cancellationTokenSource.Cancel();
            return CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTokenSource.Token);
        }
        finally
        {
            if (gate.CurrentCount is 0)
                gate.Release();
        }
    }

    void ClearDelayCancellationTokenSource(CancellationToken stoppingToken)
    {
        try
        {
            gate.Wait(stoppingToken);
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
        finally
        {
            if (gate.CurrentCount is 0)
                gate.Release();
        }
    }

    async ValueTask ExecuteJob(Job job, CancellationToken stoppingToken)
    {
        using var activity = new Activity(job.Registration.Name.ToString());
        activity.Start();
        logger.LogDebug("Executing job {JobName}", job.Registration.Name);
        await using var scope = serviceProvider.CreateAsyncScope();
        var jobInstance = (IJob) scope.ServiceProvider.GetRequiredService(job.Registration.JobType);
        var stopwatch = Stopwatch.StartNew();
        var either = jobInstance.Invoke(stoppingToken);
        stopwatch.Stop();
        await either.Match(
            _ => logger.LogTrace("Job {JobName} completed in {Elapsed}", job.Registration.Name, stopwatch.Elapsed),
            failure => logger.LogWarning("Job {JobName} failed with {Failure}", job.Registration.Name, failure));
        activity.Stop();
    }

    void ScheduleNextExecutionOfJob(Job nextJob, CancellationToken stoppingToken)
    {
        var now = clock.GetCurrentInstant();
        var nextExecutionTime = nextJob.Registration.Schedule.GetNextExecutionTime(now, isFirstExecution: false);
        try
        {
            gate.Wait(stoppingToken);
            schedule[nextJob.Registration.Name] = nextJob with { NextExecutionTime = nextExecutionTime, IsImmediate = false };
        }
        finally
        {
            if (gate.CurrentCount is 0)
                gate.Release();
        }
    }

    public Unit Queue(JobName jobName)
    {
        if (!options.Value.IsEnabled)
            return unit;
        try
        {
            logger.LogTrace("Queuing job {JobName}", jobName);
            gate.Wait();
            if (!schedule.TryGetValue(jobName, out var job))
                throw new ArgumentException($"Job {jobName} does not exist.", nameof(jobName));
            var now = clock.GetCurrentInstant();
            schedule[job.Registration.Name] = job with { NextExecutionTime = now, IsImmediate = true };
            cancellationTokenSource?.Cancel();
        }
        finally
        {
            gate.Release();
        }

        return unit;
    }

    record Job(IJobRegistration Registration, Instant NextExecutionTime, bool IsImmediate);
}
