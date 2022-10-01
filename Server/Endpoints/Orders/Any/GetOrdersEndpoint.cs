using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewOrders)]
public class GetOrdersEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<OrderSummary>>
{
    readonly Func<EitherAsync<Failure, IEnumerable<OrderSummary>>> getOrderSummaries;
    readonly ILogger logger;

    public GetOrdersEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetOrdersEndpoint> logger)
    {
        this.logger = logger;
        getOrderSummaries = () => GetOrderSummariesHandler.Handle(contextFactory);
    }

    [HttpGet("orders/any")]
    public override Task<ActionResult<IEnumerable<OrderSummary>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getOrderSummaries();
        return either.ToResultAsync(logger, HttpContext);
    }
}
