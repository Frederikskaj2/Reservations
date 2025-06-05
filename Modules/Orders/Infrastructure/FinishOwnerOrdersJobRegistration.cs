using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Orders;

class FinishOwnerOrdersJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.FinishOwnerOrders;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, LocalTime.Midnight, Duration.FromMinutes(5));
    public Type JobType => typeof(FinishOwnerOrdersJob);
}
