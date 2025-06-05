using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetOrdersShell;
using static Frederikskaj2.Reservations.Orders.Validator;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetOrdersEndpoint
{
    GetOrdersEndpoint() { }

    [Authorize(Policy = Policy.ViewOrders)]
    public static Task<IResult> Handle(
        [FromQuery] OrderType[] type,
        [FromQuery(Name = "id")] OrderId[] orderIds,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetOrdersEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        HttpContext httpContext)
    {
        var either =
            from query in ValidateGetOrders(dateProvider.Today, type, orderIds).ToAsync()
            from orderSummaries in GetOrders(options.Value, entityReader, query, httpContext.RequestAborted)
            select new GetOrdersResponse(orderSummaries.Map(CreateOrderSummary));
        return either.ToResult(logger, httpContext);
    }

    static OrderSummaryDto CreateOrderSummary(OrderSummary orderSummary) =>
        new(
            orderSummary.OrderId,
            orderSummary.Flags.HasFlag(OrderFlags.IsOwnerOrder) ? OrderType.Owner : OrderType.Resident,
            orderSummary.CreatedTimestamp,
            orderSummary.NextReservationDate,
            orderSummary.Category,
            orderSummary.Description.ToNullableReference(),
            CreateUserIdentity(orderSummary.User));
}
