using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public class TokensOptions
{
    public string Issuer { get; init; } = "https://lokaler.frederikskaj2.dk";
    public string Audience { get; init; } = "https://lokaler.frederikskaj2.dk";
    public string? AccessTokenSigningKey { get; init; }
    public Duration AccessTokenLifetime { get; init; } = Duration.FromMinutes(5);
    public Duration ClockSkew { get; init; } = Duration.Zero;
}
