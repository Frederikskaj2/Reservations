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

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class GetCreditorsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<Creditor>>
{
    readonly Func<EitherAsync<Failure, IEnumerable<Creditor>>> getDebits;
    readonly ILogger logger;

    public GetCreditorsEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetCreditorsEndpoint> logger)
    {
        this.logger = logger;
        getDebits = () => GetCreditorsHandler.Handle(contextFactory);
    }

    [HttpGet("creditors")]
    public override Task<ActionResult<IEnumerable<Creditor>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getDebits();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
