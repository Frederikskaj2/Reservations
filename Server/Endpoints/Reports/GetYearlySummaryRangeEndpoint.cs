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
public class GetYearlySummaryRangeEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<YearlySummaryRange>
{
    readonly Func<EitherAsync<Failure, YearlySummaryRange>> getYearlySummaryRange;
    readonly ILogger logger;

    public GetYearlySummaryRangeEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetYearlySummaryRangeEndpoint> logger)
    {
        this.logger = logger;
        getYearlySummaryRange = () => GetYearlySummaryRangeHandler.Handle(contextFactory, dateProvider);
    }

    [HttpGet("reports/yearly-summary/range")]
    public override Task<ActionResult<YearlySummaryRange>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getYearlySummaryRange();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
