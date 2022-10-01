using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewOrders)]
public class GetOwnerOrdersEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<Order>>
{
    readonly Func<Instant, EitherAsync<Failure, IEnumerable<Order>>> getOwnerOrders;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;

    public GetOwnerOrdersEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetOwnerOrdersEndpoint> logger)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        getOwnerOrders = timestamp => GetOwnerOrdersHandler.Handle(contextFactory, dateProvider, timestamp);
    }

    [HttpGet("orders/owner")]
    public override Task<ActionResult<IEnumerable<Order>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getOwnerOrders(getTimestamp());
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
