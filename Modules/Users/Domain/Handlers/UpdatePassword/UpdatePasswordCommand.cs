using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record UpdatePasswordCommand(Instant Timestamp, string CurrentPassword, string NewPassword, UserId UserId, TokenId TokenId);
