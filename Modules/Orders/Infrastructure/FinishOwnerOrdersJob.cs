using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.FinishOwnerOrdersShell;

namespace Frederikskaj2.Reservations.Orders;

class FinishOwnerOrdersJob(
    IDateProvider dateProvider,
    IEntityReader entityReader,
    IEntityWriter entityWriter,
    IOptionsSnapshot<OrderingOptions> options,
    ITimeConverter timeConverter)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        FinishOwnerOrders(options.Value, entityReader, timeConverter, entityWriter, new(dateProvider.Today), cancellationToken);
}
