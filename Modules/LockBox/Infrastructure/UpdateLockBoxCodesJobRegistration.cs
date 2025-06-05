using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.LockBox;

class UpdateLockBoxCodesJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.UpdateLockBoxCodes;
    public IJobSchedule Schedule => new JobSchedule(timeConverter);
    public Type JobType => typeof(UpdateLockBoxCodesJob);

    class JobSchedule(ITimeConverter timeConverter) : IJobSchedule
    {
        public Instant GetNextExecutionTime(Instant now, bool isFirstExecution)
        {
            if (isFirstExecution)
                return now.Plus(Duration.FromMinutes(1));
            var nextMonday = GetNextMonday(timeConverter.GetTime(now).Date);
            return timeConverter.GetInstant(nextMonday.AtMidnight());
        }

        static LocalDate GetNextMonday(LocalDate date) =>
            date.PlusDays(7 - ((int) date.DayOfWeek - 1));
    }
}
