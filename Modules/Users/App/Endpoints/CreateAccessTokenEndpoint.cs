using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.UpdateRefreshTokenShell;

namespace Frederikskaj2.Reservations.Users;

class CreateAccessTokenEndpoint
{
    CreateAccessTokenEndpoint() { }

    public static Task<IResult> Handle(
        [FromHeader(Name = "Cookie")] string? cookie,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<CreateAccessTokenEndpoint> logger,
        [FromServices] IOptionsSnapshot<AuthenticationOptions> options,
        [FromServices] IRefreshTokenCookieService refreshTokenCookieService,
        HttpContext httpContext
    )
    {
        var either =
            from parsedRefreshToken in refreshTokenCookieService.ParseCookie<Unit>(cookie)
            let command = new UpdateRefreshTokenCommand(dateProvider.Now, parsedRefreshToken.UserId, parsedRefreshToken.TokenId)
            from user in UpdateRefreshToken(options.Value, entityReader, entityWriter, command, httpContext.RequestAborted)
            let tokens = authenticationService.CreateTokens(user)
            let refreshTokenCookie = refreshTokenCookieService.CreateCookie(user.UserId, user.RefreshToken.TokenId, user.RefreshToken.IsPersistent)
            select WithCookies.Create(new TokensResponse(tokens.AccessToken), [refreshTokenCookie]);
        return either.ToResultWithCookie(logger, httpContext, includeFailureDetail: true);
    }
}
