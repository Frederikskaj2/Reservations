using Frederikskaj2.Reservations.Core;
using System;

namespace Frederikskaj2.Reservations.Orders;

class RemoveAccountNumbersJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.RemoveAccountNumbers;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(1, 0));
    public Type JobType => typeof(RemoveAccountNumbersJob);
}
