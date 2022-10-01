using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public class UpdateOwnerOrderRequest
{
    public string? Description { get; init; }
    public HashSet<ReservationIndex>? CancelledReservations { get; init; }
    public bool? IsCleaningRequired { get; init; }
}
