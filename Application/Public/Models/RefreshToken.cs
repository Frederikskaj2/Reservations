using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record RefreshToken(TokenId TokenId, Instant ExpireAt, bool IsPersistent, DeviceId DeviceId);
