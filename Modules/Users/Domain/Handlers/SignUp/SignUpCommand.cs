using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record SignUpCommand(Instant Timestamp, EmailAddress Email, string FullName, string Phone, Option<ApartmentId> ApartmentId, string Password);
