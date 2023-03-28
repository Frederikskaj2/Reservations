using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

static class YearlySummaryFunctions
{
    public static EitherAsync<Failure, YearlySummaryRange> GetYearlySummaryRangeOrThisYear(IPersistenceContext context, IDateProvider dateProvider) =>
        from earliestReservationDate in ReadEarliestReservationDate(context)
        select GetYearlySummaryRangeOrThisYear(dateProvider, earliestReservationDate);

    static YearlySummaryRange GetYearlySummaryRangeOrThisYear(IDateProvider dateProvider, Option<LocalDate> earliestReservationDate) =>
        earliestReservationDate.Case switch
        {
            LocalDate date => new YearlySummaryRange(date.Year, dateProvider.Today.Year),
            _ => new YearlySummaryRange(dateProvider.Today.Year, dateProvider.Today.Year)
        };

    public static EitherAsync<Failure, YearlySummary> GetYearlySummary(IPersistenceContext context, int year) =>
        from reservations in ReadReservations(context, new LocalDate(year, 1, 1), new LocalDate(year + 1, 1, 1))
        select new YearlySummary(year, GetYearlySummary(reservations));

    static IEnumerable<ResourceSummary> GetYearlySummary(IEnumerable<Reservation> reservations) =>
        reservations
            .GroupBy(reservation => Resources.GetResourceType(reservation.ResourceId).ValueUnsafe())
            .Select(GetResourceSummary);

    static ResourceSummary GetResourceSummary(IGrouping<ResourceType, Reservation> grouping)
    {
        var (reservationCount, nights, income) = grouping.Aggregate(
            (ReservationCount: 0, Nights: 0, Income: Amount.Zero),
            (sum, reservation) =>
                (sum.ReservationCount + 1, sum.Nights + reservation.Extent.Nights, sum.Income + reservation.Price!.Rent + reservation.Price.Cleaning));
        return new ResourceSummary(grouping.Key, reservationCount, nights, income);
    }
}
