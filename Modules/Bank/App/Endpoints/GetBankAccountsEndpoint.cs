using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Bank;

class GetBankAccountsEndpoint
{
    GetBankAccountsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] ILogger<GetBankAccountsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from bankAccounts in GetBankAccountsShell.GetBankAccounts(httpContext.RequestAborted)
            select new GetBankAccountsResponse(bankAccounts);
        return either.ToResult(logger, httpContext);
    }
}
