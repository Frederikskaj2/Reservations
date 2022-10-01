using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

public record ReservationsCancelledEmailModel(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    IEnumerable<ReservationDescription> Reservations,
    Amount Refund,
    Amount Fee);
