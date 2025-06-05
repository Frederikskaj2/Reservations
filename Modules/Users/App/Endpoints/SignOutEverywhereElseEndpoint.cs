using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.SignOutEverywhereElseShell;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class SignOutEverywhereElseEndpoint
{
    SignOutEverywhereElseEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromHeader] string? cookie,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SignOutEverywhereElseEndpoint> logger,
        [FromServices] IOptionsSnapshot<AuthenticationOptions> options,
        [FromServices] IRefreshTokenCookieService refreshTokenCookieService,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from parsedRefreshToken in refreshTokenCookieService.ParseCookie<Unit>(cookie)
            from _ in CheckThatAuthenticatedUserMatchesCookieUser(userId, parsedRefreshToken)
            let command = new SignOutEverywhereElseCommand(dateProvider.Now, parsedRefreshToken.UserId, parsedRefreshToken.TokenId)
            from user in SignOutEverywhereElse(options.Value, entityReader, entityWriter, command, httpContext.RequestAborted)
            let tokens = authenticationService.CreateTokens(user)
            select new TokensResponse(tokens.AccessToken);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static EitherAsync<Failure<Unit>, Unit> CheckThatAuthenticatedUserMatchesCookieUser(UserId userId, ParsedRefreshToken parsedRefreshToken) =>
        userId == parsedRefreshToken.UserId ? unit : Failure.New(HttpStatusCode.Forbidden);
}
