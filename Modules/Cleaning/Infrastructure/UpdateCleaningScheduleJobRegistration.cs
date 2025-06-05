using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Cleaning;

class UpdateCleaningScheduleJobRegistration : IJobRegistration
{
    public JobName Name => JobName.UpdateCleaningSchedule;
    public IJobSchedule Schedule => new IntervalJobSchedule(Duration.FromHours(1), Duration.FromMinutes(2));
    public Type JobType => typeof(UpdateCleaningScheduleJob);
}
