using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateMyOrderRequest(string? AccountNumber, IReadOnlyCollection<ReservationIndex>? CancelledReservations);
