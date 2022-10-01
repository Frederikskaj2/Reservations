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
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Resident))]
public class GetMyOrdersEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<MyOrder>>
{
    readonly Func<GetMyOrdersCommand, EitherAsync<Failure, IEnumerable<MyOrder>>> getMyOrders;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;

    public GetMyOrdersEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetMyOrdersEndpoint> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        getMyOrders = command => GetMyOrdersHandler.Handle(contextFactory, dateProvider, options.Value, command);
    }

    [HttpGet("orders/my")]
    public override Task<ActionResult<IEnumerable<MyOrder>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new GetMyOrdersCommand(getTimestamp(), userId)
            from orders in getMyOrders(command)
            select orders;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
