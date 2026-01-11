using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;
using static Frederikskaj2.Reservations.Bank.UpdatePayOutAccountNumberShell;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class UpdatePayOutAccountNumberEndpoint
{
    UpdatePayOutAccountNumberEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int payOutId,
        [FromBody] UpdatePayOutAccountNumberRequest request,
        [FromServices] IBankingDateProvider bankingDateProvider,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ITimeConverter timeConverter,
        [FromServices] ILogger<UpdatePayOutAccountNumberEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateUpdatePayOutAccountNumber(payOutId, request, userId, dateProvider.Now).ToAsync()
            from payOut in UpdatePayOutAccountNumber(bankingDateProvider, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new UpdatePayOutAccountNumberResponse(CreatePayOutDetails(payOut));
        return either.ToResult(logger, httpContext);
    }
}
