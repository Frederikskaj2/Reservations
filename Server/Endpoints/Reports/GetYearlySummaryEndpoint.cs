using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public class GetYearlySummaryEndpoint : EndpointBaseAsync.WithRequest<YearServerRequest>.WithActionResult<YearlySummary>
{
    readonly Func<int, EitherAsync<Failure, YearlySummary>> getYearlySummary;
    readonly ILogger logger;

    public GetYearlySummaryEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetYearlySummaryEndpoint> logger)
    {
        this.logger = logger;
        getYearlySummary = year => GetYearlySummaryHandler.Handle(contextFactory, year);
    }

    [HttpGet("reports/yearly-summary")]
    public override Task<ActionResult<YearlySummary>> HandleAsync([FromQuery] YearServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = getYearlySummary(request.Year);
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
