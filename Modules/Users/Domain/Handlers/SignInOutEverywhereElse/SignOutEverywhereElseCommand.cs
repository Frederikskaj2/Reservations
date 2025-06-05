using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record SignOutEverywhereElseCommand(Instant Timestamp, UserId UserId, TokenId TokenId);
