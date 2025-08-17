using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class ConfirmOrders
{
    public static ConfirmOrdersOutput ConfirmOrdersCore(ConfirmOrdersInput input) =>
        new(
            ConfirmOrdersByApplyingCredit(
                input.Command.Timestamp,
                input.UnconfirmedOrders,
                GetUsersMap(input.Users),
                GetLatestTransactionPerResident(input.Transactions)));

    static HashMap<UserId, User> GetUsersMap(Seq<User> users) =>
        toHashMap(users.Map(user => (user.UserId, user)));

    static HashMap<UserId, TransactionExcerpt> GetLatestTransactionPerResident(Seq<TransactionExcerpt> transactions) =>
        toHashMap(transactions.GroupBy(transaction => transaction.ResidentId, (key, values) => (key, values.OrderByDescending(t => t.Timestamp).First())));

    static Seq<UserWithOrders> ConfirmOrdersByApplyingCredit(
        Instant timestamp, Seq<Order> orders, HashMap<UserId, User> usersMap, HashMap<UserId, TransactionExcerpt> latestTransactions) =>
        orders
            .GroupBy(order => order.UserId, (key, values) => (User: usersMap[key], Orders: values.ToSeq(), LatestTransaction: latestTransactions[key]))
            .Map(tuple => new UserWithOrders(tuple.User, ConfirmOrdersByApplyingCredit(timestamp, tuple.User, tuple.Orders, tuple.LatestTransaction)))
            .Filter(userWithOrders => !userWithOrders.Orders.IsEmpty)
            .ToSeq();

    static Seq<Order> ConfirmOrdersByApplyingCredit(
        Instant timestamp, User user, Seq<Order> orders, TransactionExcerpt latestTransaction) =>
        ConfirmOrdersByApplyingCredit(
            timestamp,
            latestTransaction,
            user.Balance(),
            orders.OrderByDescending(order => order.CreatedTimestamp).ToSeq());

    static Seq<Order> ConfirmOrdersByApplyingCredit(Instant timestamp, TransactionExcerpt latestTransaction, Amount debt, Seq<Order> unconfirmedOrders) =>
        unconfirmedOrders.Fold(
                (Debt: debt, ConfirmedOrders: Seq<Order>()),
                (tuple, order) => ConfirmOrderWhenDebtHasBeenReducedToZero(timestamp, latestTransaction, order, tuple.Debt, tuple.ConfirmedOrders))
            .ConfirmedOrders;

    static (Amount Debt, Seq<Order> ConfirmedOrders) ConfirmOrderWhenDebtHasBeenReducedToZero(
        Instant timestamp, TransactionExcerpt latestTransaction, Order order, Amount debt, Seq<Order> confirmedOrders)
    {
        var (orderOption, remainingDebt) = ConfirmOrderWhenDebtHasBeenReducedToZero(timestamp, latestTransaction, debt, order);
        return (remainingDebt, TryAddOrder(confirmedOrders, orderOption));
    }

    static (Option<Order>, Amount) ConfirmOrderWhenDebtHasBeenReducedToZero(
        Instant timestamp, TransactionExcerpt latestTransaction, Amount debt, Order order) =>
        // When the debt is positive, it means that the resident owes money. The
        // unconfirmed orders that are the source of this debt are traversed newest
        // to oldest, and the next order can't be confirmed until the debt reaches
        // zero (or becomes negative). When an order can't be confirmed, it's skipped
        // and the debt is reduced by the price of that order. Eventually enough
        // orders might have been skipped reducing the remaining debt to zero, and
        // each following order is then confirmed.
        //
        // Example:
        //
        // The resident has three unpaid orders numbered 1 to 3 each having a price
        // of 1,000. Thus, the resident's debt is 3,000.
        //
        // The resident is then refunded 1,500 (settlement refunding a deposit or
        // cancellation of a paid order). The resident's debt is reduced to 1,500.
        //
        // The system processes the orders starting with order 3. As the debt
        // is above zero, it skips this order and reduces the debt from 1,000 to 500.
        //
        // As the debt is still positive, the same happens to order 2 reducing the
        // debt to -500.
        //
        // When processing order 1, the debt is no longer positive, so this order is
        // confirmed.
        (debt <= Amount.Zero ? ConfirmOrder(timestamp, latestTransaction, order) : None, debt - order.Price().Total());

    static Order ConfirmOrder(Instant timestamp, TransactionExcerpt latestTransaction, Order order) =>
        order with
        {
            Reservations = order.Reservations.Map(TryConfirmReservation),
            Audits = order.Audits.Add(OrderAudit.ConfirmOrder(timestamp, latestTransaction.AdministratorId, latestTransaction.TransactionId)),
        };

    static Reservation TryConfirmReservation(Reservation reservation) =>
        reservation.Status is ReservationStatus.Reserved
            ? reservation with { Status = ReservationStatus.Confirmed }
            : reservation;

    static Seq<Order> TryAddOrder(Seq<Order> confirmedOrders, Option<Order> orderOption) =>
        orderOption.Case switch
        {
            Order order => confirmedOrders.Add(order),
            _ => confirmedOrders,
        };
}
