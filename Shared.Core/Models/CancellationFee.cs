using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record CancellationFee(IEnumerable<ReservationIndex> Reservations);
