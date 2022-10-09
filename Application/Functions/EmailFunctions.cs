using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NodaTime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using static Frederikskaj2.Reservations.Application.CancelReservationsFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Application;

static class EmailFunctions
{
    public static EitherAsync<Failure, Unit> SendCleaningScheduleEmail(
        IPersistenceContext context, IEmailService emailService, CleaningSchedule schedule, CleaningTasksDelta delta) =>
        from users in ReadUsers(context, EmailSubscriptions.CleaningScheduleUpdated)
        from _ in emailService.Send(new CleaningScheduleEmail(schedule, delta), users).ToRightAsync<Failure, Unit>()
        select unit;

    public static EitherAsync<Failure, Unit> SendNewOrderEmail(IPersistenceContext context, IEmailService emailService, OrderId orderId) =>
        from users in ReadUsers(context, EmailSubscriptions.NewOrder)
        from _ in emailService.Send(new NewOrderEmail(orderId), users).ToRightAsync<Failure, Unit>()
        select unit;

    public static EitherAsync<Failure, Unit> SendNoFeeCancellationIsAllowed(
        IEmailService emailService, Duration duration, OrderId orderId, User user, bool isCancellationWithoutFeeAllowed) =>
        isCancellationWithoutFeeAllowed
            ? emailService
                .Send(new NoFeeCancellationAllowedModel(user.Email(), user.FullName, orderId, GetPeriod(duration)))
                .ToRightAsync<Failure, Unit>()
            : unit;

    static Period GetPeriod(Duration duration) =>
        Period.FromTicks((long) duration.TotalTicks).Normalize();

    public static EitherAsync<Failure, Unit> SendOrdersConfirmedEmail(
        IEmailService emailService, IPersistenceContext originalContext, IPersistenceContext updatedContext) =>
        SendOrdersConfirmedEmail(
            emailService,
            originalContext.Item<User>(), // User might be deleted in updated context.
            GetConfirmedOrders(originalContext.Items<Order>().ToSeq(), updatedContext.Items<Order>().ToSeq()));

    static EitherAsync<Failure, Unit> SendOrdersConfirmedEmail(IEmailService emailService, User user, Seq<Order> orders) =>
        from _ in orders.Map(order => SendOrderConfirmedEmail(emailService, user, order)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure, Unit> SendOrderConfirmedEmail(IEmailService emailService, User user, Order order) =>
        emailService.Send(new OrderConfirmedEmailModel(user.Email(), user.FullName, order.OrderId));

    public static EitherAsync<Failure, Unit> SendOrderReceivedEmail(IEmailService emailService, User user, Order order, PaymentInformation? payment) =>
        emailService
            .Send(new OrderReceivedEmailModel(user.Email(), user.FullName, order.OrderId, payment))
            .ToRightAsync<Failure, Unit>();

    public static EitherAsync<Failure, Unit> SendPayInReceivedEmail(IEmailService emailService, User user, Amount amount, PaymentInformation? payment) =>
        emailService.Send(new PayInEmailModel(user.Email(), user.FullName, amount, payment))
            .ToRightAsync<Failure, Unit>();

    public static EitherAsync<Failure, Unit> SendPayOutEmail(IEmailService emailService, User user, Amount amount) =>
        emailService.Send(new PayOutEmailModel(user.Email(), user.FullName, amount))
            .ToRightAsync<Failure, Unit>();

    public static EitherAsync<Failure, Unit> SendReservationsCancelledEmail(
        IEmailService emailService, OrderId orderId, LanguageExt.HashSet<ReservationIndex> cancelledReservations, IPersistenceContext context) =>
        !cancelledReservations.IsEmpty
            ? SendReservationsCancelledEmail(
                emailService,
                context.Order(orderId),
                context.Item<User>(),
                cancelledReservations.OrderBy(index => index).Map(index => context.Order(orderId).Reservations[index.ToInt32()]).ToSeq(),
                GetFee(context))
            : unit;

    static EitherAsync<Failure, Unit> SendReservationsCancelledEmail(
        IEmailService emailService, Order order, User user, Seq<Reservation> cancelledReservations, Amount fee) =>
            emailService.Send(
                new ReservationsCancelledEmailModel(
                    user.Email(),
                    user.FullName,
                    order.OrderId,
                    cancelledReservations.Map(reservation => new ReservationDescription(Resources.Name(reservation.ResourceId), reservation.Extent.Date)),
                    GetRefund(cancelledReservations, fee),
                    -fee));

    static Amount GetFee(IPersistenceContext context) =>
        context.ItemOption<Transaction>().Case switch
        {
            Transaction transaction => transaction.Amounts[Account.CancellationFees],
            _ => Amount.Zero
        };

    static Amount GetRefund(Seq<Reservation> cancelledReservations, Amount fee) =>
        cancelledReservations.Filter(reservation => reservation.Status == ReservationStatus.Cancelled).Sum(reservation => reservation.Price!.Total()) + fee;

    public static EitherAsync<Failure, Unit> SendReservationSettledEmail(
        IEmailService emailService, User user, OrderId orderId, Reservation reservation, Amount damages, Option<string> description) =>
        emailService.Send(
            new ReservationSettledEmailModel(
                user.Email(),
                user.FullName,
                orderId,
                new ReservationDescription(Resources.Name(reservation.ResourceId), reservation.Extent.Date),
                reservation.Price!.Deposit,
                damages,
                description.ValueUnsafe()));

    public static EitherAsync<Failure, Unit> SendUserDeletedEmail(IEmailService emailService, User user) =>
        emailService.Send(new UserDeletedEmailModel(user.Email(), user.FullName));

    static EitherAsync<Failure, IEnumerable<EmailUser>> ReadUsers(IPersistenceContext context, EmailSubscriptions subscriptions) =>
        MapReadError(
            context.Untracked.ReadItems(
                context.Query<User>()
                    .Where(user => user.EmailSubscriptions.HasFlag(subscriptions))
                    .ProjectTo(user => new EmailUser
                    {
                        UserId = user.UserId,
                        Email = user.Emails[0].Email,
                        FullName = user.FullName
                    })));

    public static EitherAsync<Failure, IEnumerable<EmailUser>> ReadEmailUsers(IPersistenceContext context, IEnumerable<UserId> userIds) =>
        MapReadError(
            context.Untracked.ReadItems(
                context.Query<User>()
                    .Where(user => userIds.Contains(user.UserId))
                    .ProjectTo(user => new EmailUser
                    {
                        UserId = user.UserId,
                        Email = user.Emails[0].Email,
                        FullName = user.FullName
                    })));

    public static EitherAsync<Failure, IEnumerable<EmailUser>> ReadEmailUsers(IPersistenceContext context, EmailSubscriptions subscriptions) =>
        MapReadError(
            context.Untracked.ReadItems(
                context.Query<User>()
                    .Where(user => user.EmailSubscriptions.HasFlag(subscriptions))
                    .ProjectTo(user => new EmailUser
                    {
                        UserId = user.UserId,
                        Email = user.Emails[0].Email,
                        FullName = user.FullName
                    })));

    public static EitherAsync<Failure<ConfirmEmailError>, UserEmail> ReadUserEmailHideNotFoundStatus(
        IPersistenceContextFactory contextFactory, EmailAddress email) =>
        MapReadErrorHideNotFound(CreateContext(contextFactory).Untracked.ReadItem<UserEmail>(EmailAddress.NormalizeEmail(email)));

    public static EitherAsync<Failure<ConfirmEmailError>, IPersistenceContext> ReadUserContext(IPersistenceContextFactory contextFactory, UserId userId) =>
        MapReadErrorHideNotFound(CreateContext(contextFactory).ReadItem<User>(User.GetId(userId)));

    public static EitherAsync<Failure<ConfirmEmailError>, IPersistenceContext> ConfirmEmailWriteContext(IPersistenceContext context) =>
        MapWriteError(context.Write());

    static EitherAsync<Failure<ConfirmEmailError>, T> MapReadErrorHideNotFound<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(
            status => Failure.New(MapReadStatusHideNotFound(status), ConfirmEmailError.InvalidRequest, $"Database read error {status}."));

    static EitherAsync<Failure<ConfirmEmailError>, T> MapWriteError<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(status => Failure.New(MapWriteStatus(status), ConfirmEmailError.Unknown, $"Database write error {status}."));

    public static EitherAsync<Failure<ConfirmEmailError>, Unit> ParseToken(
        ITokenProvider tokenProvider, Instant timestamp, ImmutableArray<byte> token, UserId userId) =>
        tokenProvider.ValidateConfirmEmailToken(timestamp, userId, token) switch
        {
            TokenValidationResult.Valid => unit,
            TokenValidationResult.Expired => Failure.New(HttpStatusCode.UnprocessableEntity, ConfirmEmailError.TokenExpired, "Token is expired."),
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, ConfirmEmailError.InvalidRequest, "Token is invalid.")
        };

    public static User ConfirmUserEmail(User user, EmailAddress email, Instant timestamp) =>
        (user with { Emails = ConfirmEmail(user.Emails, email) }) with
        {
            Audits = (user with { Emails = ConfirmEmail(user.Emails, email) }).Audits
            .Add(new UserAudit(timestamp, (user with { Emails = ConfirmEmail(user.Emails, email) }).UserId, UserAuditType.ConfirmEmail))
        };

    static Seq<EmailStatus> ConfirmEmail(Seq<EmailStatus> emails, EmailAddress email)
    {
        var emailStatus = emails.Single(status => status.NormalizedEmail == EmailAddress.NormalizeEmail(email));
        return emails.Map(status => status == emailStatus ? emailStatus with { IsConfirmed = true } : status).ToSeq();
    }
}
