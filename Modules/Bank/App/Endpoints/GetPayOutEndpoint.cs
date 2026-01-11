using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;

namespace Frederikskaj2.Reservations.Bank;

class GetPayOutEndpoint
{
    GetPayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int payOutId,
        [FromServices] IBankingDateProvider bankingDateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ITimeConverter timeConverter,
        [FromServices] ILogger<GetPayOutEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from payOut in GetPayOutShell.GetPayOut(bankingDateProvider, entityReader, timeConverter, new(payOutId), httpContext.RequestAborted)
            select new GetPayOutResponse(CreatePayOutDetails(payOut));
        return either.ToResult(logger, httpContext);
    }
}
