using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public interface IDateProvider
{
    Instant Now { get; }
    LocalDate Today { get; }
    IReadOnlySet<LocalDate> Holidays { get; }
    int GetDaysFromToday(Instant instant);
    int GetDaysFromToday(LocalDate date);
    LocalDate GetDate(Instant instant);
    LocalDateTime GetTime(Instant instant);
    Instant GetMidnight(LocalDate date);
}
