using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client;

public record CleaningScheduleDay(
    LocalDate Date,
    bool IsToday,
    bool IsCurrentMonth,
    bool IsHoliday,
    IReadOnlyDictionary<ResourceId, CleaningInterval> Intervals,
    IReadOnlySet<ResourceId> ReservedResourceIds)
{
    public static readonly IReadOnlyDictionary<ResourceId, CleaningInterval> EmptyIntervals = new Dictionary<ResourceId, CleaningInterval>();
    public static readonly IReadOnlySet<ResourceId> EmptyReservedResourceIds = new HashSet<ResourceId>();

    public override string ToString() =>
        $"{Date}, {string.Join(' ', Intervals.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}: {kvp.Value.Task}"))} {string.Join(' ', ReservedResourceIds)}";
}
