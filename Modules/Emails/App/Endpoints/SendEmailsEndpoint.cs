using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

[Authorize(Roles = nameof(Roles.Jobs))]
class SendEmailsEndpoint
{
    SendEmailsEndpoint() { }

    public static Task<IResult> Handle(
        [FromServices] IEmailDequeuer emailDequeuer,
        [FromServices] EmailSender emailSender,
        [FromServices] ILogger<SendEmailsEndpoint> logger,
        [FromServices] MessageFactory messageFactory,
        HttpContext httpContext)
    {
        var either = SendEmailsShell.SendEmails(emailDequeuer, emailSender, messageFactory, httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
