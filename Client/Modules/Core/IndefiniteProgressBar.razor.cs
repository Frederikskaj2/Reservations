using System;
using System.Diagnostics;
using System.Threading;

namespace Frederikskaj2.Reservations.Client.Modules.Core;

public sealed partial class IndefiniteProgressBar : IDisposable
{
    readonly TimeSpan progressInterval = TimeSpan.FromSeconds(0.25);
    readonly TimeSpan visibleDelay = TimeSpan.FromSeconds(0.4);
    readonly Stopwatch stopwatch = new();
    bool isVisible;
    double percentage;
    int progress;
    Timer? timer;

    public IndefiniteProgressBar() => timer = new(_ => OnTimerElapsed());

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    public void Start()
    {
        progress = 0;
        percentage = 0D;
        stopwatch.Restart();
        StartTimer();
        StateHasChanged();
    }

    public void Stop()
    {
        StopTimer();
        stopwatch.Stop();
        isVisible = false;
        StateHasChanged();
    }

    void StartTimer() => timer?.Change(progressInterval, progressInterval);

    void StopTimer() => timer?.Change(Timeout.Infinite, Timeout.Infinite);

    void OnTimerElapsed()
    {
        if (stopwatch.Elapsed >= visibleDelay)
            isVisible = true;
        if (percentage < 80D)
            percentage += (100D - percentage) / 10D;
        else if (percentage < 90D)
            percentage += (100D - percentage) / 30D;
        else
            percentage += (100D - percentage) / 60D;
        progress = (int) percentage;
        StateHasChanged();
    }
}
