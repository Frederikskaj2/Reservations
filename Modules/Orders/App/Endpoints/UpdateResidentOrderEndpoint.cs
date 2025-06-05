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
using static Frederikskaj2.Reservations.Orders.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Orders.UpdateResidentOrderShell;
using static Frederikskaj2.Reservations.Orders.Validator;

namespace Frederikskaj2.Reservations.Orders;

class UpdateResidentOrderEndpoint
{
    UpdateResidentOrderEndpoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromRoute] int orderId,
        [FromBody] UpdateResidentOrderRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IOrdersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<UpdateResidentOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var now = dateProvider.Now;
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateUpdateResidentOrder(now, OrderId.FromInt32(orderId), request, userId).ToAsync()
            from order in UpdateResidentOrder(
                emailService, jobScheduler, options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new UpdateResidentOrderResponse(CreateOrderDetails(options.Value, timeConverter.GetDate(now), order));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
