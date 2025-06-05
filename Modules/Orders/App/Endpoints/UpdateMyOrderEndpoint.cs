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
using static Frederikskaj2.Reservations.Orders.MyOrderFactory;
using static Frederikskaj2.Reservations.Orders.UpdateMyOrderShell;
using static Frederikskaj2.Reservations.Orders.Validator;

namespace Frederikskaj2.Reservations.Orders;

class UpdateMyOrderEndpoint
{
    UpdateMyOrderEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromRoute] int orderId,
        [FromBody] UpdateMyOrderRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IOrdersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<UpdateMyOrderEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateUpdateMyOrder(dateProvider.Now, OrderId.FromInt32(orderId), request, userId).ToAsync()
            from result in UpdateMyOrder(
                emailService, jobScheduler, options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new UpdateMyOrderResponse(CreateMyOrder(result.Order), result.IsUserDeleted);
        return either.ToResult(logger, httpContext);
    }
}
