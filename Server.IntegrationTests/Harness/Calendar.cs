using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

class Calendar
{
    readonly Dictionary<ResourceId, LocalDate> latestReservedDays = new();

    public Extent GetAvailableExtent(SessionFixture session, TestReservation reservation)
    {
        var latestReservedDay = latestReservedDays.TryGetValue(reservation.ResourceId, out var date) ? date : session.CurrentDate;
        var minimumDate = session.CurrentDate.PlusDays(GetDaysInTheFutureFromToday(reservation.Type, session.CurrentDate));
        var earliestDate = Maximum(latestReservedDay, minimumDate).PlusDays(reservation.AdditionalDaysInTheFuture);
        var extent = GetNextExtent(earliestDate, reservation.Nights, reservation.PriceGroup);
        latestReservedDays[reservation.ResourceId] = extent.Ends();
        return extent;
    }

    static int GetDaysInTheFutureFromToday(TestReservationType type, LocalDate date) =>
        type switch
        {
            TestReservationType.Normal => 15,
            TestReservationType.Soon => 11,
            TestReservationType.Tomorrow => 1,
            TestReservationType.Monday => GetPreviousMonday(date.PlusDays(20)).Minus(date).Days,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, message: null),
        };

    static LocalDate GetPreviousMonday(LocalDate date) =>
        date.PlusDays(-((int) date.DayOfWeek - 1));

    static Extent GetNextExtent(LocalDate earliestDate, int nights, PriceGroup priceGroup)
    {
        var holidays = HolidaysProvider.Get(earliestDate).Where(holiday => !holiday.IsOnlyBankHoliday).Select(holiday => holiday.Date).ToHashSet();
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
            _ => true,
        };

    static LocalDate Maximum(LocalDate date1, LocalDate date2) =>
        date1 >= date2 ? date1 : date2;
}
