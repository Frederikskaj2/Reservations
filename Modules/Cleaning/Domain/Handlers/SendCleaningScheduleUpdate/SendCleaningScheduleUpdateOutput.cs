using Frederikskaj2.Reservations.Persistence;
using LanguageExt;

namespace Frederikskaj2.Reservations.Cleaning;

record SendCleaningScheduleUpdateOutput(OptionalEntity<CleaningSchedule> PublishedScheduleEntity, Option<CleaningTasksDelta> DeltaOption);
