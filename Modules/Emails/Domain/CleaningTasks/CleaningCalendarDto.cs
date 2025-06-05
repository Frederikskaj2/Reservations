using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.LockBox;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Emails;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record CleaningCalendarDto(
        IEnumerable<CleaningTask> Tasks,
        IEnumerable<CleaningTask> NewTasks,
        IEnumerable<CleaningTask> CancelledTasks,
        IEnumerable<CleaningTask> UpdatedTasks,
        IReadOnlyDictionary<ResourceId, string> ResourceNames,
        IReadOnlyList<MonthCalendar> Months,
        Picture Legend);
