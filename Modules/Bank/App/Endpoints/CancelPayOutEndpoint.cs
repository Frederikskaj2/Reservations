using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.CancelPayOutShell;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;

namespace Frederikskaj2.Reservations.Bank;

class CancelPayOutEndpoint
{
    CancelPayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int payOutId,
        [FromServices] IBankingDateProvider bankingDateProvider,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ITimeConverter timeConverter,
        [FromServices] ILogger<CancelPayOutEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let command = new CancelPayOutCommand(dateProvider.Now, PayOutId.FromInt32(payOutId), userId)
            from payOut in CancelPayOut(bankingDateProvider, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new AddPayOutNoteResponse(CreatePayOutDetails(payOut));
        return either.ToResult(logger, httpContext);
    }
}
