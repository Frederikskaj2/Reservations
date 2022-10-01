using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record PayOutCommand(Instant Timestamp, UserId AdministratorUserId, UserId UserId, LocalDate Date, Amount Amount);
