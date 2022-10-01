using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Resident))]
public class UpdateMyOrderEndpoint : EndpointBaseAsync.WithRequest<UpdateMyOrderServerRequest>.WithActionResult<UpdateMyOrderResult>
{
    readonly ILogger logger;
    readonly Func<UpdateMyOrderCommand, EitherAsync<Failure, UpdateMyOrderResult>> updateOrder;
    readonly Func<OrderId, UpdateMyOrderRequest?, UserId, EitherAsync<Failure, UpdateMyOrderCommand>> validateRequest;

    public UpdateMyOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<UpdateMyOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (orderId, request, userId) => Validator.ValidateUpdateMyOrder(dateProvider, orderId, request, userId).ToAsync();
        updateOrder = command => UpdateMyOrderHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
    }

    [HttpPatch("orders/my/{orderId:int}")]
    public override Task<ActionResult<UpdateMyOrderResult>> HandleAsync(
        [FromRoute] UpdateMyOrderServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(OrderId.FromInt32(request.OrderId), request.Body, userId)
            from update in updateOrder(command)
            select update;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
