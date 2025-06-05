using Frederikskaj2.Reservations.Cleaning;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record CleaningTasksDeltaDto(
    IEnumerable<CleaningTask> NewTasks,
    IEnumerable<CleaningTask> CancelledTasks,
    IEnumerable<CleaningTask> UpdatedTasks);
