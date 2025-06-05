using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateResidentReservationsRequest(IEnumerable<ReservationUpdateRequest>? Reservations);
