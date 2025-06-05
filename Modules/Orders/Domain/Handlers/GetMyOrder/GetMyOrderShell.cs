using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;
using static Frederikskaj2.Reservations.Orders.GetMyOrder;
using static Frederikskaj2.Reservations.Orders.ResidentOrderFactory;

namespace Frederikskaj2.Reservations.Orders;

public static class GetMyOrderShell
{
    public static EitherAsync<Failure<Unit>, ResidentOrder> GetMyOrder(
        OrderingOptions options, IEntityReader reader, GetMyOrderQuery query, CancellationToken cancellationToken) =>
        from order in reader.Read<Order>(query.OrderId, cancellationToken).MapReadError()
        from user in reader.Read<User>(query.UserId, cancellationToken).MapReadError()
        from lockBoxCodes in ReadLockBoxCodes(reader, cancellationToken)
        from output in GetMyOrderCore(options, new(query, order, user, lockBoxCodes)).ToAsync()
        select CreateResidentOrder(options, query.Today, output.Order, output.User, output.LockBoxCodes);
}
