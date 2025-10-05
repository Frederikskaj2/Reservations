using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.GetYearlySummaryRange;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class GetYearlySummaryRangeShell
{
    static readonly IProjectedQuery<LocalDate> getReservationDatesQuery = Query<Order>()
        .Where(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
        .Join<Reservation>(
            order => order.Reservations,
            reservation => reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Settled)
        .Project(reservation => reservation.Extent.Date);

    public static EitherAsync<Failure<Unit>, YearlySummaryRange> GetYearlySummaryRange(
        IEntityReader entityReader, GetYearlySummaryRangeQuery query, CancellationToken cancellationToken) =>
        from earliestReservationDate in ReadEarliestReservationDate(entityReader, cancellationToken)
        let output = GetYearlySummaryRangeCore(new(query, earliestReservationDate))
        select output.YearlySummaryRange;

    static EitherAsync<Failure<Unit>, Option<LocalDate>> ReadEarliestReservationDate(IEntityReader entityReader, CancellationToken cancellationToken) =>
        from reservationDates in entityReader.Query(getReservationDatesQuery, cancellationToken).MapReadError()
        select !reservationDates.IsEmpty ? Some(reservationDates.Min()) : None;
}
