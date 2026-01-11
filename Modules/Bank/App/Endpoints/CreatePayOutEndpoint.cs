using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.CreatePayOutShell;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class CreatePayOutEndpoint
{
    CreatePayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromBody] CreatePayOutRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<CreatePayOutEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateCreatePayOut(request, userId, dateProvider.Now).ToAsync()
            from payOutSummary in CreatePayOut(entityReader, entityWriter, command, httpContext.RequestAborted)
            select new CreatePayOutResponse(CreatePayOutSummary(payOutSummary));
        return either.ToResult(logger, httpContext);
    }
}
