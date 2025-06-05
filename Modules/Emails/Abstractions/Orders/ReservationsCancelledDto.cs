using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record ReservationsCancelledDto(
    OrderId OrderId,
    IEnumerable<ReservationDescription> Reservations,
    Amount Refund,
    Amount Fee);
