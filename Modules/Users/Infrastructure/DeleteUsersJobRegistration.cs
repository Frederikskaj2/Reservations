using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Users;

class DeleteUsersJobRegistration : IJobRegistration
{
    public JobName Name => JobName.DeleteUsers;
    public IJobSchedule Schedule => new IntervalJobSchedule(Duration.FromMinutes(5));
    public Type JobType => typeof(DeleteUsersJob);
}
