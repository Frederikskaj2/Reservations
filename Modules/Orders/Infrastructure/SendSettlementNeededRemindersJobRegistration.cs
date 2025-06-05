using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Orders;

class SendSettlementNeededRemindersJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.SendSettlementNeededReminders;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(16, 0), Duration.FromMinutes(15));
    public Type JobType => typeof(SendSettlementNeededRemindersJob);
}
