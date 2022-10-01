using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public class Calendar
{
    readonly Dictionary<ResourceId, LocalDate> latestReservedDays = new();

    public Extent GetAvailableExtent(SessionFixture session, TestReservation reservation)
    {
        var latestReservedDay = latestReservedDays.TryGetValue(reservation.ResourceId, out var date) ? date : session.CurrentDate;
        var minimumDate = session.CurrentDate.PlusDays(reservation.Early ? 6 : 15);
        var earliestDate = Maximum(latestReservedDay, minimumDate).PlusDays(reservation.AdditionalDaysInTheFuture);
        var extent = GetNextExtent(earliestDate, reservation.Nights, reservation.PriceGroup);
        latestReservedDays[reservation.ResourceId] = extent.Ends();
        return extent;
    }

    static Extent GetNextExtent(LocalDate earliestDate, int nights, PriceGroup priceGroup)
    {
        var holidays = HolidaysProvider.Get(earliestDate);
        var date = earliestDate;
        while (true)
        {
            if (IsDateInPriceGroup(holidays, date, priceGroup))
                return new(date, nights);
            date = date.PlusDays(1);
        }
    }

    static bool IsDateInPriceGroup(IReadOnlySet<LocalDate> holidays, LocalDate date, PriceGroup priceGroup) =>
        priceGroup switch
        {
            PriceGroup.Low => !HighPricePolicy.IsHighPriceDay(date, holidays),
            PriceGroup.High => HighPricePolicy.IsHighPriceDay(date, holidays),
            _ => true
        };

    static LocalDate Maximum(LocalDate date1, LocalDate date2) =>
        date1 >= date2 ? date1 : date2;
}
