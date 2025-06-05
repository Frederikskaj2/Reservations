using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.GetYearlySummary;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Orders;

public static class GetYearlySummaryShell
{
    public static EitherAsync<Failure<Unit>, YearlySummary> GetYearlySummary(
        IEntityReader entityReader, GetYearlySummaryQuery query, CancellationToken cancellationToken) =>
        from reservations in ReadReservations(entityReader, new(query.Year, 1, 1), new(query.Year + 1, 1, 1), cancellationToken)
        let output = GetYearlySummaryCore(new(query, reservations))
        select new YearlySummary(output.Year, output.ResourceSummaries);

    static EitherAsync<Failure<Unit>, Seq<Reservation>> ReadReservations(
        IEntityReader entityReader, LocalDate fromDate, LocalDate toDate, CancellationToken cancellationToken) =>
        entityReader.Query(GetQuery(fromDate, toDate), cancellationToken).MapReadError();

    static IQuery<Reservation> GetQuery(LocalDate fromDate, LocalDate toDate) =>
        Query<Order>()
            .Where(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
            .Join(
                order => order.Reservations,
                reservation => (reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Settled) &&
                               fromDate <= reservation.Extent.Date && reservation.Extent.Date < toDate);
}
