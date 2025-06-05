using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client;

public record DraftReservation(Resource Resource, Extent Extent)
{
    public IEnumerable<ReservedDayDto> ReservedDays() =>
        Enumerable
            .Range(0, Extent.Nights)
            .Select(i => new ReservedDayDto(Extent.Date.PlusDays(i), Resource.ResourceId, OrderId: null, IsMyReservation: true));
}
