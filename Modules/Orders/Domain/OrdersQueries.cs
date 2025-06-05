using Frederikskaj2.Reservations.Persistence;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Orders;

public static class OrdersQueries
{
    public static readonly IQuery<Order> GetAllActiveOrdersQuery = Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));
}
