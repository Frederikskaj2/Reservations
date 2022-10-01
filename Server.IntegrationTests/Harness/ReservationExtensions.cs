using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class ReservationExtensions
{
    public static IEnumerable<MyReservedDay> ToMyReservedDays(this Reservation reservation, OrderId orderId, bool isMyReservation) =>
        Enumerable.Range(0, reservation.Extent.Nights)
            .Select(i => new MyReservedDay(reservation.Extent.Date.PlusDays(i), reservation.ResourceId, orderId, isMyReservation));
}
