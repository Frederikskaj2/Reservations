using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Cleaning;

public record SendCleaningScheduleCommand(UserId UserId, LocalDate Date);
