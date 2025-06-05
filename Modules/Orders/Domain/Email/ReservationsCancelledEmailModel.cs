using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationsCancelledEmailModel(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    IEnumerable<ReservationDescription> Reservations,
    Amount Refund,
    Amount Fee);