﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
    internal class ScheduledEmailService : IScheduledService
    {
        private readonly CleaningTaskService cleaningTaskService;
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly LockBoxCodeService lockBoxCodeService;
        private readonly EmailOptions options;
        private readonly OrderService orderService;

        public ScheduledEmailService(
            IOptions<EmailOptions> options, CleaningTaskService cleaningTaskService, IClock clock,
            DateTimeZone dateTimeZone, LockBoxCodeService lockBoxCodeService, OrderService orderService)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.cleaningTaskService = cleaningTaskService ?? throw new ArgumentNullException(nameof(cleaningTaskService));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.lockBoxCodeService = lockBoxCodeService ?? throw new ArgumentNullException(nameof(lockBoxCodeService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            this.options = options.Value;
        }

        public TimeSpan StartDelay => options.ScheduleStartDelay;

        public TimeSpan Interval => options.ScheduleInterval;

        public Task DoWork()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return Task.WhenAll(
                lockBoxCodeService.SendLockBoxCodeEmails(today),
                cleaningTaskService.SendDifferentialCleaningTasksEmail(today),
                orderService.SendOverduePaymentEmails(today),
                orderService.SendSettlementNeededEmails(today));
        }
    }
}