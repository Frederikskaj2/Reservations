using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using LanguageExt;

namespace Frederikskaj2.Reservations.Cleaning;

public sealed record CleaningSchedule(CleaningScheduleId CleaningScheduleId, Seq<CleaningTask> CleaningTasks, Seq<ReservedDay> ReservedDays) : IHasId
{
    string IHasId.GetId() => CleaningScheduleId.GetId();
}
