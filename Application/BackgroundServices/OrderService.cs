using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application.BackgroundServices;

public class OrderService
{
    readonly IPersistenceContextFactory contextFactory;
    readonly DateTimeZone dateTimeZone;
    readonly IEmailService emailService;
    readonly ILogger logger;

    public OrderService(IPersistenceContextFactory contextFactory, DateTimeZone dateTimeZone, IEmailService emailService, ILogger<OrderService> logger)
    {
        this.contextFactory = contextFactory;
        this.dateTimeZone = dateTimeZone;
        this.emailService = emailService;
        this.logger = logger;
    }

    public Task<Unit> SendSettlementNeededEmails(Instant timestamp) =>
        SendSettlementNeededEmails(contextFactory, emailService, timestamp.InZone(dateTimeZone).Date).Match(
            Right: _ => Task.FromResult(unit),
            Left: LogFailure);

    static EitherAsync<Failure, Unit> SendSettlementNeededEmails(IPersistenceContextFactory contextFactory, IEmailService emailService, LocalDate today) =>
        from context in ReadUserOrdersContext(CreateContext(contextFactory))
        let reservations = GetReservationsToSettle(context.Items<Order>(), today).ToSeq()
        from _ in TrySendSettlementNeededEmails(emailService, context, reservations)
        select unit;

    static EitherAsync<Failure, Unit> TrySendSettlementNeededEmails(
        IEmailService emailService, IPersistenceContext context, Seq<ReservationWithOrder> reservations) =>
        reservations.IsEmpty ? unit : SendSettlementNeededEmails(emailService, context, reservations);

    static EitherAsync<Failure, Unit> SendSettlementNeededEmails(
        IEmailService emailService, IPersistenceContext context, Seq<ReservationWithOrder> reservations) =>
        from users in ReadEmailUsers(context, EmailSubscriptions.SettlementRequired)
        let context1 = SetReservationEmailFlag(context, reservations, ReservationEmails.NeedsSettlement)
        from _1 in WriteContext(context1)
        from _2 in SendSettlementNeededEmails(emailService, reservations, users)
        select unit;

    static EitherAsync<Failure, Unit> SendSettlementNeededEmails(
        IEmailService emailService, Seq<ReservationWithOrder> reservations, IEnumerable<EmailUser> users) =>
        from _ in reservations
            .Map(reservation => SendSettlementNeededEmail(emailService, reservation, users))
            .TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure, Unit> SendSettlementNeededEmail(IEmailService emailService, ReservationWithOrder reservation, IEnumerable<EmailUser> users) =>
        emailService.Send(
            new SettlementNeededEmail(reservation.Order.OrderId, reservation.Reservation.ResourceId, reservation.Reservation.Extent.Ends()), users);

    void LogFailure(Failure failure) => logger.LogWarning("Sending settlement needed emails failed with {Failure}", failure);
}
