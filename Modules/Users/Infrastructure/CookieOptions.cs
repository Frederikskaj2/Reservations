using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public class CookieOptions
{
    public string? EncryptionKey { get; init; }
    public Duration DeviceCookieDuration { get; init; } = Duration.FromDays(10*365);
}
