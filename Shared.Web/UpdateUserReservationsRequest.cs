using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public class UpdateUserReservationsRequest
{
    public IEnumerable<ReservationUpdateRequest>? Reservations { get; set; }
}
