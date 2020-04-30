using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public static class HighPricePolicy
    {
        public static bool IsHighPriceDay(LocalDate date, HashSet<LocalDate> holidays)
        {
            if (holidays is null)
                throw new ArgumentNullException(nameof(holidays));

            return IsHighPriceDayOfWeek(date) || IsHolidayOrDayBeforeHoliday(date, holidays);
        }

        private static bool IsHighPriceDayOfWeek(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday
               || date.DayOfWeek == IsoDayOfWeek.Saturday
               || date.DayOfWeek == IsoDayOfWeek.Sunday;

        private static bool IsHolidayOrDayBeforeHoliday(LocalDate date, HashSet<LocalDate> holidays)
            => holidays.Contains(date) || holidays.Contains(date.PlusDays(1));
    }
}