using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.NewPasswordShell;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class NewPasswordEndpoint
{
    NewPasswordEndpoint() { }

    public static Task<IResult> Handle(
        [FromBody] NewPasswordRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<NewPasswordEndpoint> logger,
        [FromServices] IPasswordHasher passwordHasher,
        [FromServices] IPasswordValidator passwordValidator,
        [FromServices] ITokenValidator tokenValidator,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateNewPassword(dateProvider, request).ToAsync()
            from _ in NewPassword(passwordHasher, passwordValidator, entityReader, tokenValidator, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
