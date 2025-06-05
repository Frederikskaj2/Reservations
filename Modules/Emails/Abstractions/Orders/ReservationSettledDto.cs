using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Emails;

public record ReservationSettledDto(
    OrderId OrderId,
    ReservationDescription Reservation,
    Amount Deposit,
    Amount Damages,
    string? Description);
