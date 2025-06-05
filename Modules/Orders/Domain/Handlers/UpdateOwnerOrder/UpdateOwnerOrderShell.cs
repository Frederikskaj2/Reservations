using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.UpdateOwnerOrder;

namespace Frederikskaj2.Reservations.Orders;

public static class UpdateOwnerOrderShell
{
    public static EitherAsync<Failure<Unit>, OrderDetails> UpdateOwnerOrder(
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        UpdateOwnerOrderCommand command,
        CancellationToken cancellationToken) =>
        from user in reader.Read<User>(command.UserId, cancellationToken).MapReadError()
        from orderEntity in reader.ReadWithETag<Order>(command.OrderId, cancellationToken).MapReadError()
        from output in UpdateOwnerOrderCore(options, timeConverter, new(command, orderEntity.Value)).ToAsync()
        from _ in writer.Write(
            collector => collector.Add(orderEntity),
            tracker => tracker.Update(output.Order),
            cancellationToken).MapWriteError()
        from userFullNamesMap in ReadUserFullNamesMapForOrder(reader, output.Order, cancellationToken)
        select new OrderDetails(output.Order, user, userFullNamesMap);
}
