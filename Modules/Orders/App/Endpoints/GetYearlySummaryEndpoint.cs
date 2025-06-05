using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Orders;

class GetYearlySummaryEndpoint
{
    GetYearlySummaryEndpoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromQuery] int year,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetYearlySummaryEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from query in Validator.ValidateGetYearlySummary(year).ToAsync()
            from yearlySummary in GetYearlySummaryShell.GetYearlySummary(entityReader, query, httpContext.RequestAborted)
            select new GetYearlySummaryResponse(yearlySummary);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
