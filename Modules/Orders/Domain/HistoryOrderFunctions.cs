using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

static class HistoryOrderFunctions
{
    public static (User User, Order Order) TryMakeHistoryOrder(Instant timestamp, User user, Order order) =>
        ShouldBeHistoryOrder(order)
            ? (MoveToHistoryOrders(user, order.OrderId), MakeHistoryOrder(timestamp, order))
            : (user, order);

    static bool ShouldBeHistoryOrder(Order order) =>
        !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) &&
        order.Reservations.All(reservation => reservation.Status is ReservationStatus.Abandoned or ReservationStatus.Cancelled or ReservationStatus.Settled);

    static User MoveToHistoryOrders(User user, OrderId orderId) =>
            MoveToHistoryOrders(
                user,
                user.Orders.Filter(id => id != orderId),
                user.HistoryOrders.Add(orderId));

    static Order MakeHistoryOrder(Instant timestamp, Order order) =>
        order with
        {
            Flags = GetHistoryOrderFlags(order),
            Audits = order.Audits.Add(OrderAudit.FinishOrder(timestamp)),
        };

    static OrderFlags GetHistoryOrderFlags(Order order) =>
        order.Reservations.All(reservation => reservation.Status is ReservationStatus.Abandoned or ReservationStatus.Cancelled)
            ? (order.Flags | OrderFlags.IsHistoryOrder) & ~OrderFlags.IsCleaningRequired
            : order.Flags | OrderFlags.IsHistoryOrder;

    static User MoveToHistoryOrders(User user, Seq<OrderId> orders, Seq<OrderId> historyOrders) =>
        user with
        {
            Orders = orders,
            HistoryOrders = historyOrders,
        };
}
