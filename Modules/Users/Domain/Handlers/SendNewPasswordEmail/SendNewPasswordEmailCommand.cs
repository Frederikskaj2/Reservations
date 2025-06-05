using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record SendNewPasswordEmailCommand(Instant Timestamp, EmailAddress Email);
