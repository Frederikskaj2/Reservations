using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public interface ITimeConverter
{
    LocalDate GetDate(Instant instant);
    LocalDateTime GetTime(Instant instant);
    Instant GetMidnight(LocalDate date);
    Instant GetInstant(LocalDateTime time);
}