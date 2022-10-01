using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record CleaningSchedule(IEnumerable<CleaningTask> CleaningTasks, IEnumerable<ReservedDay> ReservedDays);
