using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFactory;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class GetMyOrdersShell
{
    public static EitherAsync<Failure<Unit>, Seq<ResidentOrder>> GetMyOrders(
        OrderingOptions options, IEntityReader reader, GetMyOrdersQuery query, CancellationToken cancellationToken) =>
        from user in reader.Read<User>(query.UserId, cancellationToken).MapReadError()
        from orders in ReadOrders(reader, user.Orders.Concat(user.HistoryOrders), cancellationToken)
        select CreateResidentOrders(options, query.Today, orders, user);

    static EitherAsync<Failure<Unit>, Seq<Order>> ReadOrders(IEntityReader reader, Seq<OrderId> orderIds, CancellationToken cancellationToken) =>
        !orderIds.IsEmpty
            ? reader.Query(Query<Order>().Where(order => orderIds.Contains(order.OrderId)).Project(), cancellationToken).MapReadError()
            : Seq<Order>();
}
