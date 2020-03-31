using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IDateProvider
    {
        LocalDate Today { get; }

        int GetDaysFromToday(LocalDate date);
        int GetDaysFromToday(Instant instant);
    }
}