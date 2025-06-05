using NodaTime;

namespace Frederikskaj2.Reservations.Users;

static class Authentication
{
    public static Instant GetExpireAt(AuthenticationOptions options, Instant timestamp, bool isPersistent) =>
        timestamp + (isPersistent ? options.LongRefreshTokenLifetime : options.ShortRefreshTokenLifetime);

    public static AuthenticatedUser CreateAuthenticatedUser(Instant timestamp, User user, RefreshToken refreshToken) =>
        new(timestamp, user.UserId, user.Email(), user.FullName, user.Roles, refreshToken);
}
