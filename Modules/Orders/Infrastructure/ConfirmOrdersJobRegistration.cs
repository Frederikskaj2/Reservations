using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Orders;

class ConfirmOrdersJobRegistration : IJobRegistration
{
    public JobName Name => JobName.ConfirmOrders;
    public IJobSchedule Schedule => new IntervalJobSchedule(Duration.FromMinutes(1));
    public Type JobType => typeof(ConfirmOrdersJob);
}
