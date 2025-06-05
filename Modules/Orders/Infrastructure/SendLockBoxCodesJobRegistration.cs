using Frederikskaj2.Reservations.Core;
using System;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Orders;

class SendLockBoxCodesJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.SendLockBoxCodes;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(12, 0), Duration.FromMinutes(10));
    public Type JobType => typeof(SendLockBoxCodesJob);
}
