using Microsoft.Extensions.Options;
using NodaTime;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

class ScheduledEmailService : IScheduledService
{
    readonly CleaningScheduleService cleaningScheduleService;
    readonly IClock clock;
    readonly DebtReminderService debtReminderService;
    readonly LockBoxCodesService lockBoxCodesService;
    readonly IOptionsMonitor<ScheduledEmailOptions> optionsMonitor;
    readonly OrderService orderService;

    public ScheduledEmailService(
        CleaningScheduleService cleaningScheduleService, IClock clock, DebtReminderService debtReminderService, LockBoxCodesService lockBoxCodesService,
        IOptionsMonitor<ScheduledEmailOptions> optionsMonitor, OrderService orderService)
    {
        this.cleaningScheduleService = cleaningScheduleService;
        this.clock = clock;
        this.lockBoxCodesService = lockBoxCodesService;
        this.optionsMonitor = optionsMonitor;
        this.orderService = orderService;
        this.debtReminderService = debtReminderService;
    }

    public Duration StartDelay => optionsMonitor.CurrentValue.StartDelay;
    public Duration Interval => optionsMonitor.CurrentValue.Interval;

    public Task DoWork(CancellationToken cancellationToken) =>
        DoWork(optionsMonitor.CurrentValue, clock.GetCurrentInstant());

    Task DoWork(ScheduledEmailOptions options, Instant timestamp) =>
        options.IsEnabled ? DoWork(timestamp) : Task.CompletedTask;

    async Task DoWork(Instant timestamp)
    {
        await cleaningScheduleService.SendDifferentialCleaningScheduleEmail(timestamp);
        await debtReminderService.SendDebtReminders(timestamp);
        await lockBoxCodesService.SendLockBoxCodes(timestamp);
        await orderService.SendSettlementNeededEmails(timestamp);
    }
}
