using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class MyReservedDayFactory
{
    public static IEnumerable<MyReservedDay> CreateMyReservedDays(OrderId orderId, Reservation reservation, bool isMyReservedDay) =>
        Range(0, reservation.Extent.Nights)
            .Map(i => new MyReservedDay(reservation.Extent.Date.PlusDays(i), reservation.ResourceId, orderId, isMyReservedDay));
}
