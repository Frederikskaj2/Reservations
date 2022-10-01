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
public class UpdateUserOrderEndpoint : EndpointBaseAsync.WithRequest<UpdateUserOrderServerRequest>.WithActionResult<OrderDetails>
{
    readonly ILogger logger;
    readonly Func<UpdateUserOrderCommand, EitherAsync<Failure, OrderDetails>> updateOrder;
    readonly Func<OrderId, UpdateUserOrderRequest?, UserId, EitherAsync<Failure, UpdateUserOrderCommand>> validateRequest;

    public UpdateUserOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<UpdateUserOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (orderId, request, updatedByUserid) =>
            Validator.ValidateUpdateUserOrder(dateProvider, orderId, request, updatedByUserid).ToAsync();
        updateOrder = command => UpdateUserOrderHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
    }

    [HttpPatch("orders/user/{orderId:int}")]
    public override Task<ActionResult<OrderDetails>> HandleAsync([FromRoute] UpdateUserOrderServerRequest request, CancellationToken cancellationToken)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(OrderId.FromInt32(request.OrderId), request.Body, userId)
            from order in updateOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
