using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

static class SendCleaningScheduleUpdate
{
    public static SendCleaningScheduleUpdateOutput SendCleaningScheduleUpdateCore(SendCleaningScheduleUpdateInput input) =>
        new(
            UpdateEntity(input.PublishedScheduleEntity, input.CurrentSchedule),
            GetCleaningTasksDeltaOption(
                GetCleaningTasksDelta(
                    input.Command.Date,
                    input.PublishedScheduleEntity.EntityValue.CleaningTasks,
                    input.CurrentSchedule.CleaningTasks)));

    static OptionalEntity<CleaningSchedule> UpdateEntity(OptionalEntity<CleaningSchedule> optionalEntity, CleaningSchedule schedule) =>
        optionalEntity.Match<OptionalEntity<CleaningSchedule>>(
            entity => entity with { Value = schedule },
            eTaggedEntity => eTaggedEntity with { Value = schedule });

    static CleaningTasksDelta GetCleaningTasksDelta(LocalDate date, Seq<CleaningTask> publishedTasks, Seq<CleaningTask> tasks) =>
        GetCleaningTasksDelta(date, publishedTasks, tasks, publishedTasks.Map(task => ((task.Begin, task.ResourceId), task)).ToHashMap());

    static CleaningTasksDelta GetCleaningTasksDelta(
        LocalDate date, Seq<CleaningTask> publishedTasks, Seq<CleaningTask> tasks, HashMap<(LocalDateTime, ResourceId), CleaningTask> publishedTaskMap) =>
        new(
            tasks.Except(publishedTasks, CleaningTaskComparer.Instance).ToSeq(),
            publishedTasks.Except(tasks, CleaningTaskComparer.Instance).Filter(task => task.End.Date >= date).ToSeq(),
            tasks.Filter(task => IsUpdated(publishedTaskMap, task)).ToSeq());

    static bool IsUpdated(HashMap<(LocalDateTime, ResourceId), CleaningTask> publishedTaskMap, CleaningTask task) =>
        publishedTaskMap.Find((task.Begin, task.ResourceId)).Case switch
        {
            CleaningTask matchingTask => matchingTask != task,
            _ => false,
        };

    static Option<CleaningTasksDelta> GetCleaningTasksDeltaOption(CleaningTasksDelta delta) =>
        !delta.NewTasks.IsEmpty || !delta.CancelledTasks.IsEmpty || !delta.UpdatedTasks.IsEmpty
            ? delta
            : None;
}
