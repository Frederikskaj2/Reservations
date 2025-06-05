using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record UpdateRefreshTokenCommand(Instant Timestamp, UserId UserId, TokenId TokenId);
