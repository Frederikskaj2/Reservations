using NodaTime;

namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningDayInterval(LocalDate Date, bool IsFirstDay, bool IsLastDay);
