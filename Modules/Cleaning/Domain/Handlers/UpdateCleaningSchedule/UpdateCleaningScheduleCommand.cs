using NodaTime;

namespace Frederikskaj2.Reservations.Cleaning;

public record UpdateCleaningScheduleCommand(LocalDate StartDate);
