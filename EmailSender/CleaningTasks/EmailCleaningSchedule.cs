using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.EmailSender;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record EmailCleaningSchedule(
        EmailAddress Email,
        string FullName,
        IEnumerable<CleaningTask> Tasks,
        IEnumerable<CleaningTask> NewTasks,
        IEnumerable<CleaningTask> CancelledTasks,
        IEnumerable<CleaningTask> UpdatedTasks,
        IReadOnlyDictionary<ResourceId, string> ResourceNames,
        IReadOnlyList<MonthCalendar> Months,
        Picture Legend)
    : MessageBase(Email, FullName);
