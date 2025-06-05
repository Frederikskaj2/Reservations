using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.FinishOwnerOrders;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class FinishOwnerOrdersShell
{
    static readonly IQuery<Order> query = QueryFactory
        .Query<Order>()
        .Where(order => order.Flags.HasFlag(OrderFlags.IsOwnerOrder) && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));

    public static EitherAsync<Failure<Unit>, Unit> FinishOwnerOrders(
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        FinishOwnerOrdersCommand command,
        CancellationToken cancellationToken) =>
        from ownerOrderEntities in reader.QueryWithETag(query, cancellationToken).MapReadError()
        let output = FinishOwnerOrdersCore(options, timeConverter, new(command, ownerOrderEntities.ToValues()))
        from _ in writer.Write(collector => collector.Add(ownerOrderEntities), tracker => tracker.Update(output.Orders), cancellationToken).MapWriteError()
        select unit;
}
