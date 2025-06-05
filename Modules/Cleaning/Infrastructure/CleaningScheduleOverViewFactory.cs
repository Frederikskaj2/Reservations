using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Cleaning;

static class CleaningScheduleOverViewFactory
{
    public static CleaningScheduleOverviewDto Create(CleaningSchedule schedule, CleaningTasksDelta delta) =>
        new(
            new(schedule.CleaningTasks, schedule.ReservedDays),
            new(delta.NewTasks, delta.CancelledTasks, delta.UpdatedTasks),
            Resources.All);
}
