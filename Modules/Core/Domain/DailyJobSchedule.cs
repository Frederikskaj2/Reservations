using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public class DailyJobSchedule(ITimeConverter timeConverter, LocalTime time, Duration? initialDelay = null) : IJobSchedule
{
    public Instant GetNextExecutionTime(Instant now, bool isFirstExecution)
    {
        if (isFirstExecution && initialDelay is not null)
            return now.Plus(initialDelay.Value);
        var localTime = timeConverter.GetTime(now);
        var nextExecutionTime = localTime.Date.PlusDays(localTime.TimeOfDay <= time ? 0 : 1).At(time);
        return timeConverter.GetInstant(nextExecutionTime);
    }
}