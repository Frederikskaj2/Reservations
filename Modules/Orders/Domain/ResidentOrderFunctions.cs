using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.Pricing;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class ResidentOrderFunctions
{
    public static EitherAsync<Failure<Unit>, Unit> ValidateResidentCanPlaceOrder(User user) =>
        from _1 in ValidateEmailIsConfirmed(user)
        from _2 in ValidateUserIsResident(user)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> ValidateEmailIsConfirmed(User user) =>
        user.Emails[0].IsConfirmed ? unit : Failure.New(HttpStatusCode.Forbidden, $"Email of user {user.UserId} is not confirmed.");

    static EitherAsync<Failure<Unit>, Unit> ValidateUserIsResident(User user) =>
        user.Roles.HasFlag(Roles.Resident) ? unit : Failure.New(HttpStatusCode.Forbidden, $"User {user.UserId} is not resident.");

    public static EitherAsync<Failure<Unit>, Unit> ValidateReservations(
        ReservationValidator validator,
        Seq<ReservationModel> reservations,
        Seq<Reservation> activeReservations,
        LocalDate today) =>
        reservations.Map(reservationModel => ValidateReservation(validator, today, activeReservations, reservationModel)).Sequence().Map(_ => unit).ToAsync();

    static Either<Failure<Unit>, Unit> ValidateReservation(
        ReservationValidator validator, LocalDate today, Seq<Reservation> existingReservations, ReservationModel reservation) =>
        from _1 in ValidateReservationDate(validator, today, reservation)
        from _2 in ValidateReservationDuration(validator, reservation)
        from _3 in ValidateNoConflicts(reservation, existingReservations)
        select unit;

    static Either<Failure<Unit>, Unit> ValidateReservationDate(ReservationValidator validator, LocalDate today, ReservationModel reservation) =>
        validator.IsDateWithinBounds(today, reservation.Extent.Date)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Date of {reservation} is invalid.");

    static Either<Failure<Unit>, Unit> ValidateReservationDuration(ReservationValidator validator, ReservationModel reservation) =>
        validator.IsDurationWithinBounds(reservation)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Duration of {reservation} is invalid.");

    public static EitherAsync<Failure<Unit>, Unit> SendEmails(
        IOrdersEmailService emailService,
        Seq<EmailUser> subscribedEmailUsers,
        User user,
        Order order,
        Option<PaymentInformation> payment,
        CancellationToken cancellationToken) =>
        from _1 in SendOrderReceivedEmail(emailService, user, order, payment, cancellationToken)
        from _2 in SendNewOrderEmail(emailService, subscribedEmailUsers, order.OrderId, cancellationToken)
        from _3 in SendOrdersConfirmedEmail(emailService, user, order, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendOrderReceivedEmail(
        IOrdersEmailService emailService, User user, Order order, Option<PaymentInformation> payment, CancellationToken cancellationToken) =>
        emailService
            .Send(new OrderReceivedEmailModel(user.Email(), user.FullName, order.OrderId, payment), cancellationToken)
            .ToRightAsync<Failure<Unit>, Unit>();

    static EitherAsync<Failure<Unit>, Unit> SendNewOrderEmail(
        IOrdersEmailService emailService, Seq<EmailUser> subscribedEmailUsers, OrderId orderId, CancellationToken cancellationToken) =>
        emailService.Send(new NewOrderEmailModel(orderId), subscribedEmailUsers, cancellationToken).ToRightAsync<Failure<Unit>, Unit>();

    static EitherAsync<Failure<Unit>, Unit> SendOrdersConfirmedEmail(
        IOrdersEmailService emailService, User user, Order order, CancellationToken cancellationToken) =>
        order.IsConfirmed()
            ? SendOrderConfirmedEmail(emailService, user, order, cancellationToken)
            : unit;

    public static Order CreateOrder(
        IReadOnlySet<LocalDate> holidays,
        OrderingOptions options,
        Instant timestamp,
        UserId administratorId,
        UserId residentId,
        Seq<ReservationModel> reservations,
        OrderId orderId,
        TransactionId transactionId) =>
        new(
            orderId,
            residentId,
            OrderFlags.IsCleaningRequired,
            timestamp,
            new Resident(None, Empty),
            CreateReservations(options, holidays, reservations),
            OrderAudit.PlaceResidentOrder(timestamp, administratorId, transactionId).Cons());

    static Seq<Reservation> CreateReservations(OrderingOptions options, IReadOnlySet<LocalDate> holidays, Seq<ReservationModel> reservations) =>
        reservations
            .Map(
                reservation =>
                    new Reservation(
                        reservation.ResourceId,
                        ReservationStatus.Reserved,
                        reservation.Extent,
                        GetPrice(options, holidays, reservation.Extent, reservation.ResourceType),
                        ReservationEmails.None))
            .ToSeq();

    public static Amount GetAccountsPayableToSpend(User user, Order order) =>
        Amount.Max(-order.Price().Total(), user.Accounts[Account.AccountsPayable]);

    public static Transaction CreatePlaceOrderTransaction(
        UserId administratorId, LocalDate date, Order order, TransactionId transactionId, Amount accountsPayableToSpend) =>
        new(
            transactionId,
            date,
            administratorId,
            order.CreatedTimestamp,
            Activity.PlaceOrder,
            order.UserId,
            CreateDescription(order),
            PlaceOrder(order.Price(), accountsPayableToSpend));

    static TransactionDescription CreateDescription(Order order) => new PlaceOrder(order.OrderId);
}
