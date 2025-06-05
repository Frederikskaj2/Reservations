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
using static Frederikskaj2.Reservations.Orders.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Orders.PlaceOwnerOrderShell;
using static Frederikskaj2.Reservations.Orders.Validator;

namespace Frederikskaj2.Reservations.Orders;

class PlaceOwnerOrderEndpoint
{
    PlaceOwnerOrderEndpoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromBody] PlaceOwnerOrderRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<PlaceOwnerOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var now = dateProvider.Now;
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidatePlaceOwnerOrder(now, request, userId).ToAsync()
            from order in PlaceOwnerOrder(options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new PlaceOwnerOrderResponse(CreateOrderDetails(options.Value, timeConverter.GetDate(now), order));
        return either.ToResult(logger, httpContext);
    }
}
