using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Calendar.GetReservedDays;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Calendar;

public static class GetReservedDaysShell
{
    static readonly IProjectedQuery<CalendarReservation> reservationsQuery = Query<Order>()
        .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
        .Join(
            order => order.Reservations,
            reservation => reservation.Status == ReservationStatus.Reserved || reservation.Status == ReservationStatus.Confirmed,
            (order, reservation) => new CalendarReservation(order.OrderId, order.UserId, order.Flags, reservation.ResourceId, reservation.Extent));

    public static EitherAsync<Failure<Unit>, Seq<MyReservedDay>> GetReservedDays(
        IEntityReader reader, GetReservedDaysQuery query, CancellationToken cancellationToken) =>
        from calenderReservations in reader.Query(reservationsQuery, cancellationToken).MapReadError()
        select GetReservedDaysCore(new(query, calenderReservations)).ReservedDays;
}
