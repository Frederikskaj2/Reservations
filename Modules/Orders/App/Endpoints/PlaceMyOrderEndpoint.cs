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
using static Frederikskaj2.Reservations.Orders.MyOrderFactory;
using static Frederikskaj2.Reservations.Orders.PlaceMyOrderShell;
using static Frederikskaj2.Reservations.Orders.Validator;

namespace Frederikskaj2.Reservations.Orders;

class PlaceMyOrderEndpoint
{
    PlaceMyOrderEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromBody] PlaceMyOrderRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IOrdersEmailService emailService,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<PlaceMyOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var now = dateProvider.Now;
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidatePlaceMyOrder(now, request, userId).ToAsync()
            from order in PlaceMyOrder(
                emailService,
                dateProvider.Holidays,
                jobScheduler,
                options.Value,
                entityReader,
                timeConverter,
                entityWriter,
                command,
                httpContext.RequestAborted)
            select new PlaceMyOrderResponse(CreateMyOrder(order));
        return either.ToResult(logger, httpContext);
    }
}
