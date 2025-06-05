using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceOwnerOrderRequest(string? Description, IEnumerable<ReservationRequest>? Reservations, bool IsCleaningRequired);
