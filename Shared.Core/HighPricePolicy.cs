using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class HighPricePolicy
{
    public static bool IsHighPriceDay(LocalDate date, IReadOnlySet<LocalDate> holidays) =>
        IsHighPriceDayOfWeek(date) || IsHolidayOrDayBeforeHoliday(date, holidays);

    static bool IsHighPriceDayOfWeek(LocalDate date) =>
        date.DayOfWeek == IsoDayOfWeek.Friday
        || date.DayOfWeek == IsoDayOfWeek.Saturday
        || date.DayOfWeek == IsoDayOfWeek.Sunday;

    static bool IsHolidayOrDayBeforeHoliday(LocalDate date, IReadOnlySet<LocalDate> holidays)
        => holidays.Contains(date) || holidays.Contains(date.PlusDays(1));
}
