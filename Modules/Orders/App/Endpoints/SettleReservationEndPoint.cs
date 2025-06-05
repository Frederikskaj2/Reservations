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
using static Frederikskaj2.Reservations.Orders.SettleReservationShell;
using static Frederikskaj2.Reservations.Orders.Validator;

namespace Frederikskaj2.Reservations.Orders;

class SettleReservationEndPoint
{
    SettleReservationEndPoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromRoute] int orderId,
        [FromBody] SettleReservationRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IOrdersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<SettleReservationEndPoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateSettleReservation(dateProvider.Now, orderId, request, userId).ToAsync()
            from order in SettleReservation(
                emailService, jobScheduler, options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new SettleReservationResponse(CreateOrderDetails(options.Value, timeConverter.GetDate(dateProvider.Now), order));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
