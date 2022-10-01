using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

class LockBoxCodesService
{
    readonly IPersistenceContextFactory contextFactory;
    readonly DateTimeZone dateTimeZone;
    readonly IEmailService emailService;
    readonly ILogger logger;
    readonly OrderingOptions options;

    public LockBoxCodesService(
        IPersistenceContextFactory contextFactory, DateTimeZone dateTimeZone, IEmailService emailService, ILogger<LockBoxCodesService> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.contextFactory = contextFactory;
        this.dateTimeZone = dateTimeZone;
        this.emailService = emailService;
        this.logger = logger;
        this.options = options.Value;
    }

    public Task<Unit> SendLockBoxCodes(Instant timestamp) =>
        SendLockBoxCodesEmails(contextFactory, options, emailService, timestamp.InZone(dateTimeZone).Date).Match(
            _ => Task.FromResult(unit),
            LogFailure);

    static EitherAsync<Failure, Unit> SendLockBoxCodesEmails(
        IPersistenceContextFactory contextFactory, OrderingOptions options, IEmailService emailService, LocalDate today) =>
        from context1 in ReadUserOrdersContext(CreateContext(contextFactory))
        from context2 in ReadLockBoxCodesContext(context1, today)
        let reservations = GetUpcomingReservations(context1.Items<Order>(), today.PlusDays(options.RevealLockBoxCodeDaysBeforeReservationStart)).ToSeq()
        from _ in TrySendLockBoxCodesEmails(emailService, context2, reservations)
        select unit;

    static EitherAsync<Failure, Unit> TrySendLockBoxCodesEmails(
        IEmailService emailService, IPersistenceContext context, Seq<ReservationWithOrder> reservations) =>
        reservations.IsEmpty ? unit : SendLockBoxCodes(emailService, context, reservations);

    static EitherAsync<Failure, Unit> SendLockBoxCodes(IEmailService emailService, IPersistenceContext context, Seq<ReservationWithOrder> reservations) =>
        from users in EmailFunctions.ReadEmailUsers(context, toHashSet(reservations.Map(reservation => reservation.Order.UserId)))
        let context1 = SetReservationEmailFlag(context, reservations, ReservationEmails.LockBoxCode)
        from _1 in WriteContext(context1)
        from _2 in SendLockBoxCodesEmail(emailService, reservations, context1.Item<LockBoxCodes>(), users)
        select unit;

    void LogFailure(Failure failure) => logger.LogWarning("Sending lock box codes email failed with {Failure}", failure);
}
