using NodaTime;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Frederikskaj2.Reservations.Core;

[SuppressMessage("ReSharper", "CommentTypo")]
public static class HolidaysProvider
{
    public static IReadOnlySet<Holiday> Get(LocalDate today)
    {
        var (year, month, _) = today;
        var beginningOfCurrentMonth = new LocalDate(year, month, 1);
        return GetDanishHolidaysForAYear(beginningOfCurrentMonth).ToHashSet();
    }

    static IEnumerable<Holiday> GetDanishHolidaysForAYear(LocalDate startingFrom)
    {
        // Nytårsdag
        yield return new(GetNextDate(startingFrom, 1, 1), IsOnlyBankHoliday: false);
        // Grundlovsdag
        yield return new(GetNextDate(startingFrom, 6, 5), IsOnlyBankHoliday: true);
        // Juleaftensdag
        yield return new(GetNextDate(startingFrom, 12, 25), IsOnlyBankHoliday: true);
        // Juledag
        yield return new(GetNextDate(startingFrom, 12, 25), IsOnlyBankHoliday: false);
        // 2. juledag
        yield return new(GetNextDate(startingFrom, 12, 26), IsOnlyBankHoliday: false);

        var nextYear = startingFrom.PlusYears(1);
        var thisYearHolidays = GetEasterRelatedHolidays(GetEaster(startingFrom.Year)).Where(holiday => startingFrom <= holiday.Date && holiday.Date < nextYear);
        var nextYearHolidays = GetEasterRelatedHolidays(GetEaster(nextYear.Year)).Where(holiday => startingFrom <= holiday.Date && holiday.Date < nextYear);
        foreach (var holiday in thisYearHolidays)
            yield return holiday;
        foreach (var holiday in nextYearHolidays)
            yield return holiday;
    }

    static LocalDate GetNextDate(LocalDate date, int month, int day)
    {
        var thisYear = new LocalDate(date.Year, month, day);
        return date <= thisYear ? thisYear : thisYear.PlusYears(1);
    }

    static IEnumerable<Holiday> GetEasterRelatedHolidays(LocalDate easter)
    {
        // Skærtordag
        yield return new(easter.PlusDays(days: -3), IsOnlyBankHoliday: false);
        // Langfredag
        yield return new(easter.PlusDays(days: -2), IsOnlyBankHoliday: false);
        // Påskedag
        yield return new(easter, IsOnlyBankHoliday: false);
        // 2. påskedag
        yield return new(easter.PlusDays(days: 1), IsOnlyBankHoliday: false);
        // Kristi himmelfartsdag
        yield return new(easter.PlusDays(days: 39), IsOnlyBankHoliday: false);
        // Kristi himmelfartsdag + 1
        yield return new(easter.PlusDays(days: 30), IsOnlyBankHoliday: true);
        // Pinsedag
        yield return new(easter.PlusDays(days: 49), IsOnlyBankHoliday: false);
        // 2. pinsedag
        yield return new(easter.PlusDays(days: 50), IsOnlyBankHoliday: false);
    }

    static LocalDate GetEaster(int year)
    {
        // Meeus/Jones/Butcher algorithm.
        var a = year%19;
        var b = year/100;
        var c = year%100;
        var d = b/4;
        var e = b%4;
        var f = (b + 8)/25;
        var g = (b - f + 1)/3;
        var h = (19*a + b - d - g + 15)%30;
        var i = c/4;
        var k = c%4;
        var l = (32 + 2*e + 2*i - h - k)%7;
        var m = (a + 11*h + 22*l)/451;
        var n = h + l - 7*m + 114;
        var month = n/31;
        var day = n%31 + 1;
        return new(year, month, day);
    }
}
