using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.GetBankTransactionsRangeShell;

namespace Frederikskaj2.Reservations.Bank;

class GetBankTransactionsRangeEndpoint
{
    GetBankTransactionsRangeEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetBankTransactionsRangeEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from range in GetBankTransactionsRange(entityReader, httpContext.RequestAborted)
            select new GetBankTransactionsRangeResponse(range.DateRange.ToNullableReference(), range.LatestImportStartDate.ToNullable());
        return either.ToResult(logger, httpContext);
    }
}
