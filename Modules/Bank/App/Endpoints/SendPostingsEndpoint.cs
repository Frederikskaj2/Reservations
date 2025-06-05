using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Bank;

class SendPostingsEndpoint
{
    SendPostingsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromQuery] string? month,
        [FromServices] IBankEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<SendPostingsEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from validMonth in Validator.ValidateMonth(month).ToAsync()
            let command = new SendPostingsCommand(userId, validMonth)
            from email in SendPostingsShell.SendPostings(emailService, entityReader, command, httpContext.RequestAborted)
            select email;
        return either.ToResult(logger, httpContext);
    }
}
