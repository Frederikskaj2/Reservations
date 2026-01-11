using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;

namespace Frederikskaj2.Reservations.Orders;

public static class GetOrderShell
{
    public static EitherAsync<Failure<Unit>, OrderDetails> GetOrder(IEntityReader reader, GetOrderQuery query, CancellationToken cancellationToken) =>
        from order in reader.Read<Order>(query.OrderId, cancellationToken).MapReadError()
        from user in reader.Read<User>(order.UserId, cancellationToken).MapReadError()
        from userFullNamesMap in ReadUserFullNamesMapForOrder(reader, order, cancellationToken)
        select new OrderDetails(order, user, userFullNamesMap);
}
