using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public static async Task<bool> IsHighPriceDay(LocalDate date, Func<Task<HashSet<LocalDate>>> getHolidays)
        {
            if (getHolidays is null)
                throw new ArgumentNullException(nameof(getHolidays));

            return IsHighPriceDayOfWeek(date) || IsHolidayOrDayBeforeHoliday(date, await getHolidays());
        }

        private static bool IsHighPriceDayOfWeek(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday
               || date.DayOfWeek == IsoDayOfWeek.Saturday
               || date.DayOfWeek == IsoDayOfWeek.Sunday;

        private static bool IsHolidayOrDayBeforeHoliday(LocalDate date, HashSet<LocalDate> holidays)
            => holidays.Contains(date) || holidays.Contains(date.PlusDays(1));
    }
}