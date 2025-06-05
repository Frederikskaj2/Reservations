using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.GetOrders;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class GetOrdersShell
{
    static readonly IQuery<Order> activeOrdersQuery = Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));

    public static EitherAsync<Failure<Unit>, Seq<OrderSummary>> GetOrders(
        OrderingOptions options, IEntityReader reader, GetOrdersQuery query, CancellationToken cancellationToken) =>
        from orders in reader.Query(GetOrdersQuery(query), cancellationToken).MapReadError()
        from userExcerpts in ReadUserExcerpts(reader, toHashSet(orders.Map(order => order.UserId)), cancellationToken)
        let output = GetOrdersCore(new(options, query.Today, orders, userExcerpts))
        select output.OrderSummaries;

    static IProjectedQuery<OrderExcerpt> GetOrdersQuery(GetOrdersQuery query) =>
        !query.OrderIds.IsEmpty
            ? GetOrdersQuery(query.OrderIds)
            : GetOrdersQuery(query.IncludeResidentOrders, query.IncludeOwnerOrders);

    static IProjectedQuery<OrderExcerpt> GetOrdersQuery(Seq<OrderId> orderIds) =>
        Query<Order>().Where(order => orderIds.Contains(order.OrderId))
            .Project(
                order =>
                    new OrderExcerpt(order.OrderId, order.UserId, order.Flags, order.CreatedTimestamp, order.Specifics.Owner.Description, order.Reservations));

    static IProjectedQuery<OrderExcerpt> GetOrdersQuery(bool includeResidentOrders, bool includeOwnerOrders) =>
        FilterQuery(activeOrdersQuery, includeResidentOrders, includeOwnerOrders)
            .Project(
                order =>
                    new OrderExcerpt(order.OrderId, order.UserId, order.Flags, order.CreatedTimestamp, order.Specifics.Owner.Description, order.Reservations));

    static IQuery<Order> FilterQuery(IQuery<Order> query, bool includeResidentOrders, bool includeOwnerOrders) =>
        (includeResidentOrders, includeOwnerOrders) switch
        {
            (true, false) => query.Where(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder)),
            (false, true) => query.Where(order => order.Flags.HasFlag(OrderFlags.IsOwnerOrder)),
            _ => query,
        };
}
