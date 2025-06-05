using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetMyOrderShell;
using static Frederikskaj2.Reservations.Orders.MyOrderFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetMyOrderEndpoint
{
    GetMyOrderEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromRoute] int orderId,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetMyOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let query = new GetMyOrderQuery(dateProvider.Today, userId, OrderId.FromInt32(orderId))
            from order in GetMyOrder(options.Value, entityReader, query, httpContext.RequestAborted)
            select new GetMyOrderResponse(CreateMyOrder(order));
        return either.ToResult(logger, httpContext);
    }
}
