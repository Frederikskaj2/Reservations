using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record SignOutCommand(Instant Timestamp, UserId UserId, TokenId TokenId);
