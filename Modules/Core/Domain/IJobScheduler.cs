using LanguageExt;

namespace Frederikskaj2.Reservations.Core;

public interface IJobScheduler
{
    Unit Queue(JobName jobName);
}