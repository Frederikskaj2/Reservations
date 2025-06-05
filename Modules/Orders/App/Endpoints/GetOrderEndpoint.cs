using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetOrderShell;
using static Frederikskaj2.Reservations.Orders.OrderDetailsFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetOrderEndpoint
{
    GetOrderEndpoint() { }

    [Authorize(Policy = Policy.ViewOrders)]
    public static Task<IResult> Handle(
        [FromRoute] int orderId,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        HttpContext httpContext)
    {
        var today = timeConverter.GetDate(dateProvider.Now);
        var either =
            from orderDetails in GetOrder(entityReader, new(today, OrderId.FromInt32(orderId)), httpContext.RequestAborted)
            select new GetOrderResponse(CreateOrderDetails(options.Value, today, orderDetails));
        return either.ToResult(logger, httpContext);
    }
}
