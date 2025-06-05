using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public static class LocalDateExtensions
{
    public static LocalDate MonthStart(this LocalDate date) => new(date.Year, date.Month, 1);

    public static LocalDate PreviousMonday(this LocalDate date) =>
        date.PlusDays(-((int) date.DayOfWeek - 1));
}
