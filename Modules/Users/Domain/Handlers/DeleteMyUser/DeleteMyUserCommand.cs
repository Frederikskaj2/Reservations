using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record DeleteMyUserCommand(Instant Timestamp, UserId UserId);
