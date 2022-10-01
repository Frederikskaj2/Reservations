using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record CleaningScheduleEmail(CleaningSchedule Schedule, CleaningTasksDelta Delta);
