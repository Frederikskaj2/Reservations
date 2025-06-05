using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.LockBox.SendLockBoxCodesOverviewShell;

namespace Frederikskaj2.Reservations.LockBox;

class SendLockBoxCodesOverviewEndpoint
{
    SendLockBoxCodesOverviewEndpoint() { }

    [Authorize(Roles = nameof(Roles.LockBoxCodes))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILockBoxEmailService emailService,
        [FromServices] ILogger<SendLockBoxCodesOverviewEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let command = new SendLockBoxCodesOverviewCommand(userId)
            from email in SendLockBoxCodesOverview(emailService, entityReader, command, httpContext.RequestAborted)
            select new SendLockBoxCodesResponse(email);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
