using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record CancellationFee(IEnumerable<ReservationIndex> Reservations);
