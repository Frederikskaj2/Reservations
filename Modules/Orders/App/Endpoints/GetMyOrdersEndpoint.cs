using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetMyOrdersShell;

namespace Frederikskaj2.Reservations.Orders;

class GetMyOrdersEndpoint
{
    GetMyOrdersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetMyOrdersEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let query = new GetMyOrdersQuery(dateProvider.Today, userId)
            from orders in GetMyOrders(options.Value, entityReader, query, httpContext.RequestAborted)
            select new GetMyOrdersResponse(MyOrderFactory.CreateMyOrders(orders));
        return either.ToResult(logger, httpContext);
    }
}
