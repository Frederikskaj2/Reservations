﻿using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class HolidaysProvider
    {
        private readonly IDateProvider dateProvider;

        public HolidaysProvider(IDateProvider dateProvider) => this.dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));

        public HashSet<LocalDate> GetHolidays()
        {
            var today = dateProvider.Today;
            var startingFrom = new LocalDate(today.Year, today.Month, 1);
            return GetOneYearOfDanishHolidays(startingFrom).ToHashSet();
        }

        private static IEnumerable<LocalDate> GetOneYearOfDanishHolidays(LocalDate startingFrom)
        {
            // Nytårsdag
            yield return GetNextDate(startingFrom, 1, 1);
            // Juledag
            yield return GetNextDate(startingFrom, 12, 25);
            // 2. juledag
            yield return GetNextDate(startingFrom, 12, 26);

            // Påskedag

            var nextYear = startingFrom.PlusYears(1);
            foreach (var date in GetEasterRelatedHolidays(GetEaster(startingFrom.Year)))
                if (startingFrom <= date && date < nextYear)
                    yield return date;
            foreach (var date in GetEasterRelatedHolidays(GetEaster(nextYear.Year)))
                if (startingFrom <= date && date < nextYear)
                    yield return date;
        }

        private static LocalDate GetNextDate(LocalDate date, int month, int day)
        {
            var thisYear = new LocalDate(date.Year, month, day);
            return date <= thisYear ? thisYear : thisYear.PlusYears(1);
        }

        private static IEnumerable<LocalDate> GetEasterRelatedHolidays(LocalDate easter)
        {
            // Skærtordag
            yield return easter.PlusDays(-3);
            // Langfredag
            yield return easter.PlusDays(-2);
            // påskedag
            yield return easter;
            // 2. påskedag
            yield return easter.PlusDays(1);
            // Store bededag
            yield return easter.PlusDays(26);
            // Kristi himmelfartsdag
            yield return easter.PlusDays(39);
            // Pinsedag
            yield return easter.PlusDays(49);
            // 2. pinsedag
            yield return easter.PlusDays(50);
        }

        private static LocalDate GetEaster(int year)
        {
            // Meeus/Jones/Butcher algorithm.
            var a = year%19;
            var b = year/100;
            var c = year%100;
            var d = b/4;
            var e = b%4;
            var f = (b + 8)/25;
            var g = (b - f + 1)/3;
            var h = (19*a + b -d - g + 15)%30;
            var i = c/4;
            var k = c%4;
            var l = (32 + 2*e + 2*i - h- k)%7;
            var m = (a + 11*h + 22*l)/451;
            var n = h + l - 7*m + 114;
            var month = n/31;
            var day = n%31 + 1;
            return new LocalDate(year, month, day);
        }
    }
}