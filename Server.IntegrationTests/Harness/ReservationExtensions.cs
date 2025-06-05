using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class ReservationExtensions
{
    public static IEnumerable<ReservedDayDto> ToMyReservedDays(this ReservationDto reservation, OrderId orderId, bool isMyReservation) =>
        GenerateReservedDays(reservation.Extent.Date, reservation.Extent.Nights, reservation.ResourceId, orderId, isMyReservation);

    public static IEnumerable<ReservedDayDto> GenerateReservedDays(LocalDate date, int nights, ResourceId resourceId, OrderId orderId, bool isMyReservation) =>
        Enumerable.Range(0, nights)
            .Select(i => new ReservedDayDto(date.PlusDays(i), resourceId, orderId, isMyReservation));
}
