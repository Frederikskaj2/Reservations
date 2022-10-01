using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewOrders)]
public class GetOrderEndpoint : EndpointBaseAsync.WithRequest<OrderServerRequest>.WithActionResult<OrderDetails>
{
    readonly Func<GetOrderCommand, EitherAsync<Failure, OrderDetails>> getOrderDetails;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;

    public GetOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetOrderEndpoint> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        getOrderDetails = command => GetOrderDetailsHandler.Handle(contextFactory, dateProvider, options.Value, command);
    }

    [HttpGet("orders/any/{orderId:int}")]
    public override Task<ActionResult<OrderDetails>> HandleAsync([FromRoute] OrderServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = getOrderDetails(new GetOrderCommand(getTimestamp(), request.OrderId));
        return either.ToResultAsync(logger, HttpContext);
    }
}
