using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Shared.Core.Pricing;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class PlaceUserOrderFunctions
{
    public static EitherAsync<Failure, Unit> ValidateUserCanPlaceOrder(User user) =>
        from _1 in ValidateEmailIsConfirmed(user)
        from _2 in ValidateUserIsResident(user)
        select unit;

    static EitherAsync<Failure, Unit> ValidateEmailIsConfirmed(User user) =>
        user.Emails[0].IsConfirmed ? unit : Failure.New(HttpStatusCode.Forbidden, $"Email of user {user.UserId} is not confirmed.");

    static EitherAsync<Failure, Unit> ValidateUserIsResident(User user) =>
        user.Roles.HasFlag(Roles.Resident) ? unit : Failure.New(HttpStatusCode.Forbidden, $"User {user.UserId} is not resident.");

    public static IPersistenceContext PlaceUserOrder(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, PlaceUserOrderCommand command, LocalDate today, IPersistenceContext context, OrderId orderId,
        TransactionId transactionId) =>
        PlaceOrder(options, command, today, context, transactionId, CreateOrder(options, holidays, command, orderId, transactionId));

    static IPersistenceContext PlaceOrder(
        OrderingOptions options, PlaceUserOrderCommand command, LocalDate today, IPersistenceContext context, TransactionId transactionId, Order order) =>
        ScheduleCleaning(
            options,
            ApplyCreditToOrders(
                command.Timestamp,
                command.UserId,
                AddUserTransaction(
                    UpdateUser(command, AddOrder(context, order), order.OrderId),
                    CreatePlaceOrderTransaction(today, order, transactionId, GetAccountsPayableToSpend(context, order))),
                transactionId));

    static Amount GetAccountsPayableToSpend(IPersistenceContext context, Order order) =>
        Amount.Max(-order.Price().Total(), context.Item<User>().Accounts[Account.AccountsPayable]);

    static IPersistenceContext AddOrder(IPersistenceContext context, Order order) =>
        context.CreateItem(order, o => Order.GetId(o.OrderId));

    static Order CreateOrder(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, PlaceUserOrderCommand command, OrderId orderId, TransactionId transactionId) =>
        new(
            orderId,
            command.UserId,
            OrderFlags.IsCleaningRequired,
            command.Timestamp,
            new UserOrder(command.ApartmentId, null, Empty),
            null,
            CreateReservations(options, holidays, command.Reservations),
            CreateAudit(command, transactionId).Cons());

    static Seq<Reservation> CreateReservations(OrderingOptions options, IReadOnlySet<LocalDate> holidays, Seq<ReservationModel> reservations) =>
        reservations
            .Map(reservation =>
                new Reservation(
                    reservation.ResourceId,
                    ReservationStatus.Reserved,
                    reservation.Extent,
                    GetPrice(options, holidays, reservation.Extent, reservation.ResourceType),
                    ReservationEmails.None,
                    null))
            .ToSeq();

    static OrderAudit CreateAudit(PlaceUserOrderCommand command, TransactionId transactionId) =>
        new(command.Timestamp, command.AdministratorUserId, OrderAuditType.PlaceOrder, transactionId);

    static IPersistenceContext UpdateUser(PlaceUserOrderCommand command, IPersistenceContext context, OrderId orderId) =>
        context.UpdateItem<User>(user => UpdateUser(command, user, orderId));

    static User UpdateUser(PlaceUserOrderCommand command, User user, OrderId orderId) =>
        user
            .UpdateApartmentId(command.Timestamp, command.ApartmentId)
            .SetAccountNumber(command.Timestamp, command.AccountNumber, user.UserId)
            .UpdateFullName(command.Timestamp, command.FullName, user.UserId)
            .UpdatePhone(command.Timestamp, command.Phone, user.UserId)
            .AddOrderToUser(command.Timestamp, command.AdministratorUserId, orderId);

    static User AddOrderToUser(this User user, Instant timestamp, UserId updatedByUserId, OrderId orderId) =>
        user with
        {
            Orders = user.Orders.Add(orderId),
            Audits = user.Audits.Add(UserAudit.Create(timestamp, updatedByUserId, UserAuditType.CreateOrder, orderId))
        };
}
