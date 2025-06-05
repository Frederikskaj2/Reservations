using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateOwnerOrderRequest(string? Description, IReadOnlyCollection<ReservationIndex>? CancelledReservations, bool? IsCleaningRequired);
