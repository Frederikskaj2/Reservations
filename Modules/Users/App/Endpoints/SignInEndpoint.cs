using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class SignInEndpoint
{
    SignInEndpoint() { }

    public static Task<IResult> Handle(
        [FromHeader(Name = "Cookie")] string? cookie,
        [FromBody] SignInRequest request,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IDeviceCookieService deviceCookieService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SignInEndpoint> logger,
        [FromServices] IOptionsSnapshot<AuthenticationOptions> options,
        [FromServices] IPasswordHasher passwordHasher,
        [FromServices] IRefreshTokenCookieService refreshTokenCookieService,
        HttpContext httpContext)
    {
        var either =
            from deviceId in RightAsync<Failure<SignInError>, Option<DeviceId>>(deviceCookieService.ParseCookie(cookie))
            from command in ValidateSignIn(dateProvider, request, deviceId).ToAsync()
            from user in SignInShell.SignIn(options.Value, passwordHasher, entityReader, entityWriter, command, httpContext.RequestAborted)
            let tokens = authenticationService.CreateTokens(user)
            let deviceCookie = deviceCookieService.CreateCookie(user.RefreshToken.DeviceId)
            let refreshTokenCookie = refreshTokenCookieService.CreateCookie(user.UserId, user.RefreshToken.TokenId, user.RefreshToken.IsPersistent)
            select WithCookies.Create(new TokensResponse(tokens.AccessToken), [deviceCookie, refreshTokenCookie]);
        return either.ToResultWithCookie(logger, httpContext, includeFailureDetail: true);
    }
}
