using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Web;

public class ReservationUpdateRequest
{
    public ReservationIndex ReservationIndex { get; set; }
    public Extent Extent { get; set; }
}
