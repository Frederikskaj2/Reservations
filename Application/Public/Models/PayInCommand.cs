using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record PayInCommand(Instant Timestamp, UserId AdministratorUserId, PaymentId PaymentId, LocalDate Date, Amount Amount);
