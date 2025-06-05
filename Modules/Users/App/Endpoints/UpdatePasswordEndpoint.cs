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
using static Frederikskaj2.Reservations.Users.UpdatePasswordShell;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class UpdatePasswordEndpoint
{
    UpdatePasswordEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromHeader(Name = "Cookie")] string? cookie,
        [FromBody] UpdatePasswordRequest request,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<UpdatePasswordEndpoint> logger,
        [FromServices] IOptionsSnapshot<AuthenticationOptions> options,
        [FromServices] IPasswordHasher passwordHasher,
        [FromServices] IPasswordValidator passwordValidator,
        [FromServices] IRefreshTokenCookieService refreshTokenCookieService,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden, PasswordError.Unknown))
            from parsedRefreshToken in refreshTokenCookieService.ParseCookie(cookie, PasswordError.Unknown)
            from _ in CheckThatAuthenticatedUserMatchesCookieUser(userId, parsedRefreshToken)
            from command in ValidateUpdatePassword(dateProvider, request, parsedRefreshToken).ToAsync()
            from user in UpdatePassword(options.Value, passwordHasher, passwordValidator, entityReader, entityWriter, command, httpContext.RequestAborted)
            let tokens = authenticationService.CreateTokens(user)
            let refreshTokenCookie = refreshTokenCookieService.CreateCookie(user.UserId, user.RefreshToken.TokenId, user.RefreshToken.IsPersistent)
            select WithCookies.Create(new TokensResponse(tokens.AccessToken), [refreshTokenCookie]);
        return either.ToResultWithCookie(logger, httpContext, includeFailureDetail: true);
    }

    static EitherAsync<Failure<PasswordError>, Unit> CheckThatAuthenticatedUserMatchesCookieUser(UserId userId, ParsedRefreshToken parsedRefreshToken) =>
        userId == parsedRefreshToken.UserId ? unit : Failure.New(HttpStatusCode.Forbidden, PasswordError.Unknown);
}
