using Frederikskaj2.Reservations.Core;
using System;

namespace Frederikskaj2.Reservations.Bank;

class SendDebtRemindersJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.SendDebtReminders;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(6, 0));
    public Type JobType => typeof(SendDebtRemindersJob);
}
