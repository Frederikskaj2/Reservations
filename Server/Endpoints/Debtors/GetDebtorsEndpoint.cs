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
public class GetDebtorsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<Debtor>>
{
    readonly Func<EitherAsync<Failure, IEnumerable<Debtor>>> getDebtors;
    readonly ILogger logger;

    public GetDebtorsEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetDebtorsEndpoint> logger)
    {
        this.logger = logger;
        getDebtors = () => GetDebtorsHandler.Handle(contextFactory);
    }

    [HttpGet("debtors")]
    public override Task<ActionResult<IEnumerable<Debtor>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getDebtors();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
