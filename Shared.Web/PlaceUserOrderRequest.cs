using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public class PlaceUserOrderRequest
{
    public UserId UserId { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ApartmentId? ApartmentId { get; set; }
    public string? AccountNumber { get; set; }
    public IEnumerable<ReservationRequest>? Reservations { get; set; }
}
