using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.SendNewPasswordEmailShell;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class SendNewPasswordEmailEndpoint
{
    SendNewPasswordEmailEndpoint() { }

    public static Task<IResult> Handle(
        [FromBody] SendNewPasswordEmailRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IUsersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SendNewPasswordEmailEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateSendNewPasswordEmail(dateProvider, request).ToAsync()
            from _ in SendNewPasswordEmail(emailService, entityReader, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
