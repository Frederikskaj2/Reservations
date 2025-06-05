using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.GetPostingsRangeShell;

namespace Frederikskaj2.Reservations.Bank;

class GetPostingsRangeEndpoint
{
    GetPostingsRangeEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetPostingsRangeEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from monthRange in GetPostingsRange(entityReader, new(dateProvider.Today), httpContext.RequestAborted)
            select new GetPostingsRangeResponse(monthRange);
        return either.ToResult(logger, httpContext);
    }
}
