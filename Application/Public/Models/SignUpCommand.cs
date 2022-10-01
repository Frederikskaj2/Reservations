using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record SignUpCommand(Instant Timestamp, EmailAddress Email, string FullName, string Phone, Option<ApartmentId> ApartmentId, string Password);
