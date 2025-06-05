using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record NewPasswordEmailModel(Instant Timestamp, EmailAddress Email, string FullName);
