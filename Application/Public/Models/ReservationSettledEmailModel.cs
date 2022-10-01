using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record ReservationSettledEmailModel(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    ReservationDescription Reservation,
    Amount Deposit,
    Amount Damages,
    string? DamagesDescription);
