using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public class SignInEndpoint : EndpointBaseAsync.WithRequest<SignInServerRequest>.WithActionResult<Tokens>
{
    readonly Func<AuthenticatedUser, Cookie> createDeviceCookie;
    readonly Func<AuthenticatedUser, Cookie> createRefreshTokenCookie;
    readonly Func<AuthenticatedUser, Tokens> createTokens;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure<SignInError>, Option<DeviceId>>> parseDeviceCookie;
    readonly Func<SignInCommand, EitherAsync<Failure<SignInError>, AuthenticatedUser>> signIn;
    readonly Func<SignInRequest?, Option<DeviceId>, EitherAsync<Failure<SignInError>, SignInCommand>> validateRequest;

    public SignInEndpoint(
        AuthenticationService authenticationService, IPersistenceContextFactory contextFactory, IDateProvider dateProvider, DeviceCookieService deviceCookieService,
        ILogger<SignInEndpoint> logger, IOptionsSnapshot<AuthenticationOptions> options, IPasswordHasher passwordHasher,
        RefreshTokenCookieService refreshTokenCookieService)
    {
        this.logger = logger;
        createDeviceCookie = user => deviceCookieService.CreateCookie(user.RefreshToken.DeviceId);
        createRefreshTokenCookie = user => refreshTokenCookieService.CreateCookie(user.UserId, user.RefreshToken.TokenId, user.RefreshToken.IsPersistent);
        parseDeviceCookie = cookie => RightAsync<Failure<SignInError>, Option<DeviceId>>(deviceCookieService.ParseCookie(cookie));
        validateRequest = (request, deviceId) => Validator.ValidateSignIn(dateProvider, request, deviceId).ToAsync();
        signIn = command => SignInHandler.Handle(contextFactory, options.Value, passwordHasher, command);
        createTokens = authenticationService.CreateTokens;
    }

    [HttpPost("user/sign-in")]
    public override Task<ActionResult<Tokens>> HandleAsync([FromRoute] SignInServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from deviceId in parseDeviceCookie(request.Cookie)
            from command in validateRequest(request.ClientRequest, deviceId)
            from user in signIn(command)
            let tokens = createTokens(user)
            let deviceCookie = createDeviceCookie(user)
            let refreshTokenCookie = createRefreshTokenCookie(user)
            select new WithCookies<Tokens>(tokens, new[] { deviceCookie, refreshTokenCookie });
        return either.ToResultWithCookieAsync(logger, HttpContext, true);
    }
}
