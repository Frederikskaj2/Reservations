using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class ScheduledEmailService : IScheduledService
    {
        public TimeSpan Interval => TimeSpan.FromMinutes(5);

        public Task DoWork() => Task.CompletedTask;
    }
}