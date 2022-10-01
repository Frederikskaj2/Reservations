using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class CleaningScheduleTable
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<CleaningTask>? CleaningTasks { get; set; }
    [Parameter] public IReadOnlyDictionary<ResourceId, Resource>? Resources { get; set; }
}
