using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class ResendConfirmEmailEmailEndpoint
{
    ResendConfirmEmailEmailEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IUsersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<ResendConfirmEmailEmailEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let command = new ResendConfirmEmailEmailCommand(dateProvider.Now, userId)
            from _ in ResendConfirmEmailEmailShell.ResendConfirmEmailEmail(emailService, entityReader, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
