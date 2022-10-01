using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewOrders)]
public class GetUserOrdersEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<Order>>
{
    readonly Func<EitherAsync<Failure, IEnumerable<Order>>> getUserOrders;
    readonly ILogger logger;

    public GetUserOrdersEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetUserOrdersEndpoint> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getUserOrders = () => GetUserOrdersHandler.Handle(contextFactory, dateProvider, options.Value);
    }

    [HttpGet("orders/user")]
    public override Task<ActionResult<IEnumerable<Order>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getUserOrders();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
