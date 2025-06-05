using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public static class LocalDateExtensions
{
    public static LocalDateTime At(this LocalDate date, LocalTime time) =>
        new(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
}
