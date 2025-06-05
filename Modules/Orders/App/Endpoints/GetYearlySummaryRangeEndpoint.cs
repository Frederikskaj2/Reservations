using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Orders;

class GetYearlySummaryRangeEndpoint
{
    GetYearlySummaryRangeEndpoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetYearlySummaryRangeEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from yearlySummaryRange in GetYearlySummaryRangeShell.GetYearlySummaryRange(entityReader, new(dateProvider.Today), httpContext.RequestAborted)
            select new GetYearlySummaryRangeResponse(yearlySummaryRange);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
