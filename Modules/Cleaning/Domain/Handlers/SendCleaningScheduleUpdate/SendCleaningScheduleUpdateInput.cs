using Frederikskaj2.Reservations.Persistence;

namespace Frederikskaj2.Reservations.Cleaning;

record SendCleaningScheduleUpdateInput(
    SendCleaningScheduleUpdateCommand Command,
    CleaningSchedule CurrentSchedule,
    OptionalEntity<CleaningSchedule> PublishedScheduleEntity);