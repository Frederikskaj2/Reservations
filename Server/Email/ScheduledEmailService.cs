using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class ScheduledEmailService : IScheduledService
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly KeyCodeService keyCodeService;

        public ScheduledEmailService(IClock clock, DateTimeZone dateTimeZone, KeyCodeService keyCodeService)
        {
            this.clock = clock;
            this.dateTimeZone = dateTimeZone;
            this.keyCodeService = keyCodeService;
        }

        public TimeSpan Interval => TimeSpan.FromHours(6);

        public Task DoWork()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return keyCodeService.SendKeyCodeEmails(today);
        }
    }
}