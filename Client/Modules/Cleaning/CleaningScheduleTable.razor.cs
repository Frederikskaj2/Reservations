using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.LockBox;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client.Modules.Cleaning;

public partial class CleaningScheduleTable
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<CleaningTask>? CleaningTasks { get; set; }
    [Parameter] public IReadOnlyDictionary<ResourceId, Resource>? Resources { get; set; }
}
