using NodaTime;

namespace Frederikskaj2.Reservations.Application;

static class Authentication
{
    public static Instant GetExpireAt(AuthenticationOptions options, Instant timestamp, bool isPersistent) =>
        timestamp + (isPersistent ? options.LongRefreshTokenLifetime : options.ShortRefreshTokenLifetime);
}