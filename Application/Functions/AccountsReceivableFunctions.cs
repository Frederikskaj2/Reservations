using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DebtReminderFunctions;
using static Frederikskaj2.Reservations.Application.UserBalanceFunctions;

namespace Frederikskaj2.Reservations.Application;

static class AccountsReceivableFunctions
{
    public static IPersistenceContext AddUserTransaction(IPersistenceContext context, Transaction transaction) =>
        context
            .CreateItem(transaction, t => Transaction.GetId(t.TransactionId))
            .UpdateItem<User>(user => user with { Accounts = ValidateUserAccounts(user.Accounts.Apply(transaction.Amounts)) });

    // Balance in favor of the user (credit) can be used to confirm orders.
    public static IPersistenceContext ApplyCreditToOrders(
        Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId) =>
        TrySetLatestDebtReminder(
            ApplyCreditToOrders(timestamp, updatedByUserId, context, transactionId, context.Item<User>()),
            timestamp);

    static IPersistenceContext ApplyCreditToOrders(Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId, User user) =>
        ApplyDebtToOrders(
            timestamp,
            updatedByUserId,
            context,
            transactionId,
            user.Balance(),
            GetUnconfirmedOrdersNewestFirst(user.UserId, context));

    static Seq<Order> GetUnconfirmedOrdersNewestFirst(UserId userId, IPersistenceContext context) =>
        context.Items<Order>()
            .Filter(order => order.UserId == userId && order.NeedsConfirmation())
            .OrderByDescending(order => order.CreatedTimestamp)
            .ToSeq();

    static IPersistenceContext ApplyDebtToOrders(
        Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId, Amount debt, Seq<Order> unconfirmedOrders) =>
        unconfirmedOrders.Fold(
                (Context: context, Debt: debt),
                (tuple, order) => ApplyDebtToOrders(timestamp, updatedByUserId, tuple.Context, transactionId, tuple.Debt, order))
            .Context;

    static (IPersistenceContext, Amount) ApplyDebtToOrders(
        Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId, Amount debt, Order order) =>
        // Start confirming orders when the debt becomes negative as
        // the unconfirmed orders are traversed newest to oldest and
        // the debt is reduced with the price of each order as it's
        // processed.
        debt <= Amount.Zero
            ? (ConfirmOrder(timestamp, updatedByUserId, order.OrderId, context, transactionId), debt - order.Price().Total())
            : (context, debt - order.Price().Total());

    // When money is paid by the user it's debited to their balance.
    public static IPersistenceContext ApplyDebitToOrders(Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId) =>
        TryClearLatestDebtReminder(
            ApplyDebitToOrders(timestamp, updatedByUserId, context, transactionId, GetUnconfirmedOrdersOldestFirst(context.Item<User>().UserId, context)));

    static Seq<Order> GetUnconfirmedOrdersOldestFirst(UserId userId, IPersistenceContext context) =>
        context.Items<Order>().Filter(order => order.UserId == userId && order.NeedsConfirmation()).OrderBy(order => order.CreatedTimestamp).ToSeq();

    static IPersistenceContext ApplyDebitToOrders(
        Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId, Seq<Order> unconfirmedOrders) =>
        ApplyDebitToOrders(
            timestamp,
            updatedByUserId,
            context,
            transactionId,
            unconfirmedOrders,
            GetCredit(context, unconfirmedOrders));

    // Balance in favor of the user (credit) can be used to confirm orders.
    static Amount GetCredit(IPersistenceContext context, Seq<Order> unconfirmedOrders) =>
        context.Item<User>().Balance() - unconfirmedOrders.Fold(Amount.Zero, (amount, order) => amount + order.Price().Total());

    static IPersistenceContext ApplyDebitToOrders(
        Instant timestamp, UserId updatedByUserId, IPersistenceContext context, TransactionId transactionId, Seq<Order> unconfirmedOrders, Amount credit) =>
        unconfirmedOrders.Fold(
                (Context: context, Credit: credit),
                (tuple, order) => ApplyCreditToOrders(tuple.Context, timestamp, updatedByUserId, tuple.Credit, transactionId, order))
            .Context;

    static (IPersistenceContext, Amount) ApplyCreditToOrders(
        IPersistenceContext context, Instant timestamp, UserId updatedByUserId, Amount credit, TransactionId transactionId, Order order) =>
        // Credit values are negative so invert the sign before comparing to a positive amount.
        order.Price().Total() <= -credit
            ? (ConfirmOrder(timestamp, updatedByUserId, order.OrderId, context, transactionId), credit + order.Price().Total())
            : (context, credit + order.Price().Total());

    static IPersistenceContext ConfirmOrder(
        Instant timestamp, UserId updatedByUserId, OrderId orderId, IPersistenceContext context, TransactionId transactionId) =>
        context.UpdateItem<Order>(Order.GetId(orderId), order => ConfirmOrder(timestamp, updatedByUserId, order, transactionId));

    static Order ConfirmOrder(Instant timestamp, UserId updateByUserId, Order order, TransactionId transactionId) =>
        order with
        {
            Reservations = order.Reservations.Map(TryConfirmReservation).ToSeq(),
            Audits = order.Audits.Add(new(timestamp, updateByUserId, OrderAuditType.ConfirmOrder, transactionId))
        };

    static Reservation TryConfirmReservation(Reservation reservation) =>
        reservation.Status == ReservationStatus.Reserved
            ? reservation with { Status = ReservationStatus.Confirmed }
            : reservation;
}
