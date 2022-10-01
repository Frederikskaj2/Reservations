using NodaTime;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

public class ScheduledEmailOptions
{
    public bool IsEnabled { get; set; }
    public Duration StartDelay { get; init; } = Duration.FromMinutes(1);
    public Duration Interval { get; init; } = Duration.FromHours(6);
}
