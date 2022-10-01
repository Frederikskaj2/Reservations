using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record CleaningTasksDelta(IEnumerable<CleaningTask> NewTasks, IEnumerable<CleaningTask> CancelledTasks, IEnumerable<CleaningTask> UpdatedTasks);
