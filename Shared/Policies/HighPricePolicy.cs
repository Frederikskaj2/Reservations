using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public static class HighPricePolicy
    {
        public static bool IsHighPriceDay(LocalDate date, HashSet<LocalDate> holidays)
            => IsHighPriceWeekDay(date) || holidays.Contains(date) || holidays.Contains(date.PlusDays(1));

        public static async Task<bool> IsHighPriceDay(LocalDate date, Func<Task<HashSet<LocalDate>>> getHolidays)
        {
            if (IsHighPriceWeekDay(date))
                return true;
            var holidays = await getHolidays();
            return holidays.Contains(date) || holidays.Contains(date.PlusDays(1));
        }

        private static bool IsHighPriceWeekDay(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday
               || date.DayOfWeek == IsoDayOfWeek.Saturday
               || date.DayOfWeek == IsoDayOfWeek.Sunday;
    }
}