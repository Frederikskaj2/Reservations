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

[Authorize(Roles = nameof(Roles.OrderHandling))]
public class UpdateOwnerOrderEndpoint : EndpointBaseAsync.WithRequest<UpdateOwnerOrderServerRequest>.WithActionResult<OrderDetails>
{
    readonly ILogger logger;
    readonly Func<UpdateOwnerOrderCommand, EitherAsync<Failure, OrderDetails>> updateOrder;
    readonly Func<OrderId, UpdateOwnerOrderRequest?, UserId, EitherAsync<Failure, UpdateOwnerOrderCommand>> validateRequest;

    public UpdateOwnerOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<UpdateOwnerOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (orderId, request, userId) => Validator.ValidateUpdateOwnerOrder(dateProvider, orderId, request, userId).ToAsync();
        updateOrder = command => UpdateOwnerOrderHandler.Handle(contextFactory, dateProvider, options.Value, command);
    }

    [HttpPatch("orders/owner/{orderId:int}")]
    public override Task<ActionResult<OrderDetails>> HandleAsync([FromRoute] UpdateOwnerOrderServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(OrderId.FromInt32(request.OrderId), request.Body, userId)
            from order in updateOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
