using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public class UpdateMyOrderRequest
{
    public string? AccountNumber { get; set; }
    public HashSet<ReservationIndex>? CancelledReservations { get; set; }
    public bool WaiveFee { get; set; }
}
