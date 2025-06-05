using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Orders.OrdersLockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.PaymentFunctions;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class PlaceMyOrder
{
    public static PlaceMyOrderOutput PlaceMyOrderCore(IReadOnlySet<LocalDate> holidays, OrderingOptions options, PlaceMyOrderInput input) =>
        CreateOutput(
            options,
            input,
            CreateOrder(
                holidays,
                options,
                input.Command.Timestamp,
                input.Command.UserId,
                input.Command.UserId,
                input.Command.Reservations,
                input.OrderId,
                input.TransactionId));

    static PlaceMyOrderOutput CreateOutput(OrderingOptions options, PlaceMyOrderInput input, Order order) =>
        CreateOutput(
            options,
            input,
            order,
            CreatePlaceOrderTransaction(input.Command.UserId, input.Date, order, input.TransactionId, GetAccountsPayableToSpend(input.User, order)));

    static PlaceMyOrderOutput CreateOutput(OrderingOptions options, PlaceMyOrderInput input, Order order, Transaction transaction) =>
        CreateOutput(options, input, order, transaction, UpdateResident(input.Command, input.User, order.OrderId, transaction));

    static User UpdateResident(PlaceMyOrderCommand command, User user, OrderId orderId, Transaction transaction) =>
        user
            .UpdateApartmentId(command.Timestamp, command.ApartmentId, user.UserId)
            .SetAccountNumber(command.Timestamp, command.AccountNumber, user.UserId)
            .UpdateFullName(command.Timestamp, command.FullName, user.UserId)
            .UpdatePhone(command.Timestamp, command.Phone, user.UserId)
            .AddOrderToUser(orderId)
            .AddOrderAudit(command.Timestamp, orderId)
            .AddTransaction(transaction)
            .SetLatestDebtReminder(command.Timestamp);

    static User AddOrderToUser(this User user, OrderId orderId) =>
        user with { Orders = user.Orders.Add(orderId) };

    static User AddOrderAudit(this User user, Instant timestamp, OrderId orderId) =>
        user with { Audits = user.Audits.Add(UserAudit.CreateOrder(timestamp, user.UserId, orderId)) };

    static User SetLatestDebtReminder(this User user, Instant timestamp) =>
        user.HasDebt()
            ? user with { LatestDebtReminder = timestamp }
            : user with { LatestDebtReminder = None };

    static PlaceMyOrderOutput CreateOutput(OrderingOptions options, PlaceMyOrderInput input, Order order, Transaction transaction, User updatedUser) =>
        new(
            updatedUser,
            order,
            transaction,
            GetPaymentInformation(options, updatedUser),
            CreateLockBoxCodesForOrder(options, input.Date, order, input.LockBoxCodes));
}
