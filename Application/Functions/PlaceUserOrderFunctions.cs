using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
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
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, PlaceMyOrderCommand command, LocalDate today, IPersistenceContext context, OrderId orderId,
        TransactionId transactionId) =>
        PlaceOrder(options, command, today, context, transactionId, CreateOrder(options, holidays, command, orderId, transactionId));

    static IPersistenceContext PlaceOrder(
        OrderingOptions options, PlaceMyOrderCommand command, LocalDate today, IPersistenceContext context, TransactionId transactionId, Order order) =>
        ScheduleCleaning(
            options,
            ApplyCreditToOrders(
                command.Timestamp,
                command.UserId,
                AddUserTransaction(
                    UpdateUser(command, AddOrder(context, order), order.OrderId),
                    CreatePlaceOrderTransaction(today, order, transactionId, GetUserAccountsPayable(context))),
                transactionId));

    static Amount GetUserAccountsPayable(IPersistenceContext context) =>
        context.Item<User>().Accounts[Account.AccountsPayable];

    static IPersistenceContext AddOrder(IPersistenceContext context, Order order) =>
        context.CreateItem(order, o => Order.GetId(o.OrderId));

    static Order CreateOrder(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, PlaceMyOrderCommand command, OrderId orderId, TransactionId transactionId) =>
        new(
            orderId,
            command.UserId,
            OrderFlags.IsCleaningRequired,
            command.Timestamp,
            new UserOrder(command.ApartmentId, null, Empty),
            null,
            CreateReservations(options, holidays, command),
            CreateAudit(command, transactionId).Cons());

    static Seq<Reservation> CreateReservations(OrderingOptions options, IReadOnlySet<LocalDate> holidays, PlaceMyOrderCommand command) =>
        command.Reservations
            .Map(reservation =>
                new Reservation(
                    reservation.ResourceId,
                    ReservationStatus.Reserved,
                    reservation.Extent,
                    Pricing.GetPrice(options, holidays, reservation.Extent, reservation.ResourceType),
                    ReservationEmails.None,
                    null))
            .ToSeq();

    static OrderAudit CreateAudit(PlaceMyOrderCommand command, TransactionId transactionId) =>
        new(command.Timestamp, command.UserId, OrderAuditType.PlaceOrder, transactionId);

    static IPersistenceContext UpdateUser(PlaceMyOrderCommand command, IPersistenceContext context, OrderId orderId) =>
        context.UpdateItem<User>(user => UpdateUser(command, user, orderId));

    static User UpdateUser(PlaceMyOrderCommand command, User user, OrderId orderId) =>
        user
            .UpdateApartmentId(command.Timestamp, command.ApartmentId)
            .SetAccountNumber(command.Timestamp, command.AccountNumber, user.UserId)
            .UpdateFullName(command.Timestamp, command.FullName, user.UserId)
            .UpdatePhone(command.Timestamp, command.Phone, user.UserId)
            .AddOrderToUser(command.Timestamp, orderId);

    static User AddOrderToUser(this User user, Instant timestamp, OrderId orderId) =>
        user with
        {
            Orders = user.Orders.Add(orderId),
            Audits = user.Audits.Add(UserAudit.Create(timestamp, user.UserId, UserAuditType.CreateOrder, orderId))
        };
}
