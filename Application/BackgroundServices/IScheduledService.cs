using NodaTime;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

interface IScheduledService
{
    Duration StartDelay { get; }
    Duration Interval { get; }
    Task DoWork(CancellationToken cancellationToken);
}
