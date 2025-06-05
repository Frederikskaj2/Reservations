using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationSettledEmailModel(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    ReservationDescription Reservation,
    Amount Deposit,
    Amount Damages,
    string? DamagesDescription);