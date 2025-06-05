using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record ConfirmEmailEmailModel(Instant Timestamp, UserId UserId, EmailAddress Email, string FullName);
