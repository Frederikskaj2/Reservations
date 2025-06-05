using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Emails;

class SendEmailsJobRegistration : IJobRegistration
{
    public JobName Name => JobName.SendEmails;
    public IJobSchedule Schedule => new IntervalJobSchedule(Duration.FromMinutes(1));
    public Type JobType => typeof(SendEmailsJob);
}
