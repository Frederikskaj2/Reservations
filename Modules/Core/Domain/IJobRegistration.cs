using System;

namespace Frederikskaj2.Reservations.Core;

public interface IJobRegistration
{
    JobName Name { get; }
    IJobSchedule Schedule { get; }
    Type JobType { get; }
}
