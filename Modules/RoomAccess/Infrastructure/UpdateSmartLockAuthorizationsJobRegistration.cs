using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.RoomAccess;

class UpdateSmartLockAuthorizationsJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.UpdateSmartLockAuthorizations;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(13, 0), Duration.FromMinutes(10));
    public Type JobType => typeof(UpdateSmartLockAuthorizationsJob);
}
