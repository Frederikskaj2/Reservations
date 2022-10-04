using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

static class HistoryOrderFunctions
{
    public static IPersistenceContext TryMakeHistoryOrder(Instant timestamp, UserId userId, OrderId orderId, IPersistenceContext context) =>
        TryMakeHistoryOrder(timestamp, userId, context, context.Order(orderId));

    static IPersistenceContext TryMakeHistoryOrder(Instant timestamp, UserId userId, IPersistenceContext context, Order order) =>
        ShouldBeHistoryOrder(order) ? MakeHistoryOrder(timestamp, userId, context, order) : context;

    static bool ShouldBeHistoryOrder(Order order) =>
        !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) &&
        order.Reservations.All(reservation =>
            reservation.Status is ReservationStatus.Abandoned or ReservationStatus.Cancelled or ReservationStatus.Settled);

    static IPersistenceContext MakeHistoryOrder(Instant timestamp, UserId userId, IPersistenceContext context, Order order) =>
        context
            .UpdateItem<Order>(Order.GetId(order.OrderId), o => MakeHistoryOrder(timestamp, o))
            .UpdateItem<User>(user => MoveToHistoryOrders(timestamp, userId, order.OrderId.Cons(), user));

    static Order MakeHistoryOrder(Instant timestamp, Order order) =>
        order with
        {
            Flags = GetHistoryOrderFlags(order),
            Audits = order.Audits.Add(new(timestamp, null, OrderAuditType.FinishOrder))
        };

    static OrderFlags GetHistoryOrderFlags(Order order) =>
        order.Reservations.All(reservation => reservation.Status is ReservationStatus.Abandoned or ReservationStatus.Cancelled)
            ? (order.Flags | OrderFlags.IsHistoryOrder) & ~OrderFlags.IsCleaningRequired
            : order.Flags | OrderFlags.IsHistoryOrder;

    static User MoveToHistoryOrders(Instant timestamp, UserId updatedByUserId, Seq<OrderId> orderId, User user) =>
        TryRemoveAccountNumber(
            timestamp,
            updatedByUserId,
            MoveToHistoryOrders(
                user,
                user.Orders.Except(orderId).ToSeq(),
                user.HistoryOrders.Concat(orderId).ToSeq()));

    static User MoveToHistoryOrders(User user, Seq<OrderId> orders, Seq<OrderId> historyOrders) =>
        user with
        {
            Orders = orders,
            HistoryOrders = historyOrders
        };
}
