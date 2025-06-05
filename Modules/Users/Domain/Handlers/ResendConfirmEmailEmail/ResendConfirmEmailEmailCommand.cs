using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record ResendConfirmEmailEmailCommand(Instant Timestamp, UserId UserId);
