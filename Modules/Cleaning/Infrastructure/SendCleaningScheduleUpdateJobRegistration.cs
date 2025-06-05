using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Cleaning;

class SendCleaningScheduleUpdateJobRegistration : IJobRegistration
{
    public JobName Name => JobName.SendCleaningScheduleUpdate;
    public IJobSchedule Schedule => new IntervalJobSchedule(Duration.FromHours(2), Duration.FromMinutes(3));
    public Type JobType => typeof(SendCleaningScheduleUpdateJob);
}
