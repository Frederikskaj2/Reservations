using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using LanguageExt;

namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningSchedule(CleaningScheduleId CleaningScheduleId, Seq<CleaningTask> CleaningTasks, Seq<ReservedDay> ReservedDays) : IHasId
{
    public string GetId() => CleaningScheduleId.GetId();
}
