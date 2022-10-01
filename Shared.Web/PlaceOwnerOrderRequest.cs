using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public class PlaceOwnerOrderRequest
{
    public string? Description { get; init; }
    public IEnumerable<ReservationRequest>? Reservations { get; init; }
    public bool IsCleaningRequired { get; init; }
}