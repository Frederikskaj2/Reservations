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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Resident))]
public class GetMyOrderEndpoint : EndpointBaseAsync.WithRequest<OrderServerRequest>.WithActionResult<MyOrder>
{
    readonly Func<GetMyOrderCommand, EitherAsync<Failure, MyOrder>> getMyOrder;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;

    public GetMyOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetMyOrderEndpoint> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        getMyOrder = command => GetMyOrderHandler.Handle(contextFactory, dateProvider, options.Value, command);
    }

    [HttpGet("orders/my/{orderId:int}")]
    public override Task<ActionResult<MyOrder>> HandleAsync([FromRoute] OrderServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new GetMyOrderCommand(getTimestamp(), userId, OrderId.FromInt32(request.OrderId))
            from order in getMyOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext);
    }
}
