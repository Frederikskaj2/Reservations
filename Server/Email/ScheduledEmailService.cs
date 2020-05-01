using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    internal class ScheduledEmailService : IScheduledService
    {
        private readonly CleaningTaskService cleaningTaskService;
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly KeyCodeService keyCodeService;
        private readonly OrderService orderService;

        public ScheduledEmailService(
            CleaningTaskService cleaningTaskService, IClock clock, DateTimeZone dateTimeZone,
            KeyCodeService keyCodeService, OrderService orderService)
        {
            this.cleaningTaskService = cleaningTaskService ?? throw new ArgumentNullException(nameof(cleaningTaskService));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.keyCodeService = keyCodeService ?? throw new ArgumentNullException(nameof(keyCodeService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public TimeSpan Interval => TimeSpan.FromHours(6);

        public Task DoWork()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return Task.WhenAll(
                keyCodeService.SendKeyCodeEmails(today),
                cleaningTaskService.SendDifferentialCleaningTasksEmail(today),
                orderService.SendOverduePaymentEmails(today),
                orderService.SendSettlementNeededEmails(today));
        }
    }
}