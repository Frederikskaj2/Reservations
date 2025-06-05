using NodaTime;

namespace Frederikskaj2.Reservations.Cleaning;

public record SendCleaningScheduleUpdateCommand(LocalDate Date);