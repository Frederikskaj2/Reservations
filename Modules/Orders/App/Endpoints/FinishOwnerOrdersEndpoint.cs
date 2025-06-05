using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.FinishOwnerOrdersShell;

namespace Frederikskaj2.Reservations.Orders;

class FinishOwnerOrdersEndpoint
{
    FinishOwnerOrdersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        IDateProvider dateProvider,
        IEntityReader entityReader,
        IEntityWriter entityWriter,
        ILogger<FinishOwnerOrdersEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options,
        ITimeConverter timeConverter,
        HttpContext httpContext)
    {
        var command = new FinishOwnerOrdersCommand(dateProvider.Today);
        var either = FinishOwnerOrders(options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
