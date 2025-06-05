using LanguageExt;

namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningTasksDelta(Seq<CleaningTask> NewTasks, Seq<CleaningTask> CancelledTasks, Seq<CleaningTask> UpdatedTasks)
{
    public static readonly CleaningTasksDelta Empty = new(Seq<CleaningTask>.Empty, Seq<CleaningTask>.Empty, Seq<CleaningTask>.Empty);
}
