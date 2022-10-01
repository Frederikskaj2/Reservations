using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.DebtReminderFunctions;
using static Frederikskaj2.Reservations.Application.PaymentFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

class DebtReminderService
{
    readonly IPersistenceContextFactory contextFactory;
    readonly IEmailService emailService;
    readonly ILogger logger;
    readonly OrderingOptions options;

    public DebtReminderService(
        IPersistenceContextFactory contextFactory, IEmailService emailService, ILogger<DebtReminderService> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.contextFactory = contextFactory;
        this.emailService = emailService;
        this.logger = logger;
        this.options = options.Value;
    }

    public Task<Unit> SendDebtReminders(Instant timestamp) =>
        SendDebtReminders(contextFactory, options, emailService, timestamp).Match(
            Right: _ => Task.FromResult(unit),
            Left: LogFailure);

    static EitherAsync<Failure, Unit> SendDebtReminders(
        IPersistenceContextFactory contextFactory, OrderingOptions options, IEmailService emailService, Instant timestamp) =>
        from context1 in ReadUsersToRemindAboutDebtContext(options, timestamp, CreateContext(contextFactory))
        let users = context1.Items<User>()
        let context2 = UpdateLatestDebtReminders(context1, timestamp)
        from _1 in users.Map(user => SendDebtReminder(options, emailService, user)).TraverseSerial(identity)
        from _2 in WriteContext(context2)
        select unit;

    static EitherAsync<Failure, Unit> SendDebtReminder(OrderingOptions options, IEmailService emailService, User user) =>
        from _ in emailService.Send(new DebtReminderEmailModel(user.Email(), user.FullName, GetPaymentInformation(options, user)))
        select unit;

    void LogFailure(Failure failure) => logger.LogWarning("Sending debt reminder emails failed with {Failure}", failure);
}
