using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

class CleaningScheduleService
{
    readonly IPersistenceContextFactory contextFactory;
    readonly DateTimeZone dateTimeZone;
    readonly IEmailService emailService;
    readonly ILogger logger;
    readonly OrderingOptions options;

    public CleaningScheduleService(
        IPersistenceContextFactory contextFactory, DateTimeZone dateTimeZone, IEmailService emailService, ILogger<CleaningScheduleService> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.contextFactory = contextFactory;
        this.dateTimeZone = dateTimeZone;
        this.emailService = emailService;
        this.logger = logger;
        this.options = options.Value;
    }

    public Task<Unit> SendDifferentialCleaningScheduleEmail(Instant timestamp) =>
        TryGetCleaningScheduleDelta(contextFactory, options, timestamp.InZone(dateTimeZone).Date).Match(
            Right: tuple => SendEmail(tuple.Schedule, tuple.Delta),
            Left: LogFailure);

    Task<Unit> SendEmail(CleaningSchedule schedule, Option<CleaningTasksDelta> optionalDelta) =>
        optionalDelta.Case switch
        {
            CleaningTasksDelta delta => SendEmail(schedule, delta),
            _ => Task.FromResult(unit)
        };

    Task<Unit> SendEmail(CleaningSchedule schedule, CleaningTasksDelta delta) =>
        SendCleaningScheduleEmail(DatabaseFunctions.CreateContext(contextFactory), emailService, schedule, delta).Match(
            Right: _ => Task.FromResult(unit),
            Left: LogFailure);

    void LogFailure(Failure failure) => logger.LogWarning("Sending cleaning schedule email failed with {Failure}", failure);
}
