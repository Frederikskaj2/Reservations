using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.SignOutShell;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class SignOutEndpoint
{
    SignOutEndpoint() { }

    public static Task<IResult> Handle(
        [FromHeader] string? cookie,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SignOutEndpoint> logger,
        [FromServices] IRefreshTokenCookieService refreshTokenCookieService,
        HttpContext httpContext)
    {
        var either =
            from parsedRefreshToken in refreshTokenCookieService.ParseCookie<Unit>(cookie)
            let command = new SignOutCommand(dateProvider.Now, parsedRefreshToken.UserId, parsedRefreshToken.TokenId)
            from _ in SignOut(entityReader, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
