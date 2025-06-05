using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record RefreshToken(TokenId TokenId, Instant ExpireAt, bool IsPersistent, DeviceId DeviceId);
