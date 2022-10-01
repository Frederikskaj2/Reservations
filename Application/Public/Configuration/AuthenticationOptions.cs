using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public class AuthenticationOptions
{
    public Duration ShortRefreshTokenLifetime { get; init; } = Duration.FromHours(1);
    public Duration LongRefreshTokenLifetime { get; init; } = Duration.FromDays(30);
    public int MaximumAllowedFailedSignInAttempts { get; init; } = 10;
    public Duration LockoutPeriod { get; init; } = Duration.FromMinutes(5);
}