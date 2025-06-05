using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record UpdateMyUserCommand(Instant Timestamp, UserId UserId, string FullName, string Phone, EmailSubscriptions EmailSubscriptions);
