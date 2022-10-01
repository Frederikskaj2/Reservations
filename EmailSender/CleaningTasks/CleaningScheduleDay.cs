using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

public record CleaningScheduleDay(LocalDate Date, IReadOnlyDictionary<ResourceId, CleaningInterval> Intervals, IReadOnlySet<ResourceId> ReservedResourceIds)
{
    public static readonly IReadOnlyDictionary<ResourceId, CleaningInterval> EmptyIntervals = new Dictionary<ResourceId, CleaningInterval>();
    public static readonly IReadOnlySet<ResourceId> EmptyReservedResourceIds = new HashSet<ResourceId>();
}
