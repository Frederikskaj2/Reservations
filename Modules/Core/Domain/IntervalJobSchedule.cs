using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public class IntervalJobSchedule(Duration interval, Duration? firstInterval = null) : IJobSchedule
{
    public Instant GetNextExecutionTime(Instant now, bool isFirstExecution) =>
        now.Plus(isFirstExecution ? firstInterval ?? interval : interval);
}