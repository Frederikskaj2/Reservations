using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public interface IJobSchedule
{
    Instant GetNextExecutionTime(Instant now, bool isFirstExecution);
}
