using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Calendar.GetOwnerReservedDays;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Calendar;

public static class GetOwnerReservedDaysShell
{
    public static EitherAsync<Failure<Unit>, Seq<MyReservedDay>> GetOwnerReservedDays(
        OrderingOptions options, IEntityReader reader, GetOwnerReservedDaysQuery query, CancellationToken cancellationToken) =>
        from currentReservations in reader.Query(GetCurrentOrdersQuery(), cancellationToken).MapReadError()
        from recentReservations in reader.Query(GetRecentOrdersQuery(query.Today.Minus(options.RecentOrdersMaximumAge)), cancellationToken).MapReadError()
        let allReservations = currentReservations.Concat(recentReservations)
        select GetOwnerReservedDaysCore(new(query, allReservations)).ReservedDays;

    static IQuery<CalendarReservation> GetCurrentOrdersQuery() =>
        Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
            .Join(
                order => order.Reservations,
                reservation => reservation.Status == ReservationStatus.Reserved || reservation.Status == ReservationStatus.Confirmed,
                (order, reservation) => new CalendarReservation(order.OrderId, order.UserId, order.Flags, reservation.ResourceId, reservation.Extent));

    static IQuery<CalendarReservation> GetRecentOrdersQuery(LocalDate onOrAfter) =>
        Query<Order>()
            .Where(order => order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
            .Join(
                order => order.Reservations,
                reservation => reservation.Status == ReservationStatus.Settled && reservation.Extent.Date >= onOrAfter,
                (order, reservation) => new CalendarReservation(order.OrderId, order.UserId, order.Flags, reservation.ResourceId, reservation.Extent));
}
