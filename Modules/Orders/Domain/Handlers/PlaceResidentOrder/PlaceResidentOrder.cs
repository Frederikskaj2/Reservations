using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Orders.OrdersLockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.PaymentFunctions;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class PlaceResidentOrder
{
    public static PlaceResidentOrderOutput PlaceResidentOrderCore(IReadOnlySet<LocalDate> holidays, OrderingOptions options, PlaceResidentOrderInput input)
    {
        var placeResidentOrderOutput = CreateOutput(
            options,
            input,
            CreateOrder(
                holidays,
                options,
                input.Command.Timestamp,
                input.Command.AdministratorId,
                input.Command.ResidentId,
                input.Command.Reservations,
                input.OrderId,
                input.TransactionId));
        return placeResidentOrderOutput;
    }

    static PlaceResidentOrderOutput CreateOutput(OrderingOptions options, PlaceResidentOrderInput input, Order order) =>
        CreateOutput(
            options,
            input,
            order,
            CreatePlaceOrderTransaction(input.Command.AdministratorId, input.Date, order, input.TransactionId, GetAccountsPayableToSpend(input.User, order)));

    static PlaceResidentOrderOutput CreateOutput(OrderingOptions options, PlaceResidentOrderInput input, Order order, Transaction transaction) =>
        CreateOutput(
            options,
            input,
            order,
            transaction,
            UpdateResident(input.Command, input.User, order.OrderId, transaction),
            UpdateAdministrator(input.Command, input.Administrator, input.OrderId));

    static User UpdateResident(PlaceResidentOrderCommand command, User user, OrderId orderId, Transaction transaction) =>
        user
            .UpdateApartmentId(command.Timestamp, command.ApartmentId, command.AdministratorId)
            .SetAccountNumber(command.Timestamp, command.AccountNumber, command.AdministratorId)
            .UpdateFullName(command.Timestamp, command.FullName, command.AdministratorId)
            .UpdatePhone(command.Timestamp, command.Phone, command.AdministratorId)
            .AddOrderToUser(orderId)
            .AddTransaction(transaction)
            .SetLatestDebtReminder(command.Timestamp);

    static User AddOrderToUser(this User user, OrderId orderId) =>
        user with { Orders = user.Orders.Add(orderId) };

    static User SetLatestDebtReminder(this User user, Instant timestamp) =>
        user.HasDebt()
            ? user with { LatestDebtReminder = timestamp }
            : user with { LatestDebtReminder = None };

    static PlaceResidentOrderOutput CreateOutput(
        OrderingOptions options, PlaceResidentOrderInput input, Order order, Transaction transaction, User updatedUser, User updatedAdministrator) =>
        new(
            updatedAdministrator,
            updatedUser,
            order,
            transaction,
            GetPaymentInformation(options, updatedUser),
            CreateLockBoxCodesForOrder(options, input.Date, order, input.LockBoxCodes));

    static User UpdateAdministrator(PlaceResidentOrderCommand command, User user, OrderId orderId) =>
        user.AddOrderAudit(command.Timestamp, orderId);

    static User AddOrderAudit(this User user, Instant timestamp, OrderId orderId) =>
        user with { Audits = user.Audits.Add(UserAudit.CreateOrder(timestamp, user.UserId, orderId)) };
}
