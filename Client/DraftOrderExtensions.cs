using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client;

static class DraftOrderExtensions
{
    public static IEnumerable<MyReservedDay> ReservedDays(this DraftOrder order)
        => order.Reservations.SelectMany(ReservedDays);

    public static IEnumerable<MyReservedDay> ReservedDays(this DraftReservation reservation) =>
        Enumerable
            .Range(0, reservation.Extent.Nights)
            .Select(i => new MyReservedDay(reservation.Extent.Date.PlusDays(i), reservation.Resource.ResourceId, null, true));
}
