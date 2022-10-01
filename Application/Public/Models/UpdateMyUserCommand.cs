using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateMyUserCommand(Instant Timestamp, UserId UserId, string FullName, string Phone, EmailSubscriptions EmailSubscriptions);
