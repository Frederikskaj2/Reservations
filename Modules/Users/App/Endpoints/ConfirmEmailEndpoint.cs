using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.ConfirmEmailShell;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class ConfirmEmailEndpoint
{
    ConfirmEmailEndpoint() { }

    public static Task<IResult> Handle(
        [FromBody] ConfirmEmailRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<ConfirmEmailEndpoint> logger,
        [FromServices] ITokenValidator tokenValidator,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateConfirmEmail(dateProvider, request).ToAsync()
            from _ in ConfirmEmail(entityReader, tokenValidator, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext);
    }
}
