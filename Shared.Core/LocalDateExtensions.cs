using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class LocalDateExtensions
{
    public static LocalDate MonthStart(this LocalDate date) => new(date.Year, date.Month, 1);

    public static LocalDate PreviousMonday(this LocalDate date)
    {
        var daysAfterMonday = ((int) date.DayOfWeek - 1)%7;
        return date.PlusDays(-daysAfterMonday);
    }
}
