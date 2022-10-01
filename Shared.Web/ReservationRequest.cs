using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Web;

public class ReservationRequest
{
    public ResourceId ResourceId { get; set; }
    public Extent Extent { get; set; }
}
