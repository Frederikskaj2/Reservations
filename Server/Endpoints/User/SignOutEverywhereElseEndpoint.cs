using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class SignOutEverywhereElseEndpoint : EndpointBaseAsync.WithRequest<RefreshTokenServerRequest>.WithActionResult<Tokens>
{
    readonly Func<AuthenticatedUser, Tokens> createTokens;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure, ParsedRefreshToken>> parseRefreshToken;
    readonly Func<RefreshTokenCommand, EitherAsync<Failure, AuthenticatedUser>> signOutEverywhereElse;

    public SignOutEverywhereElseEndpoint(
        AuthenticationService authenticationService, IPersistenceContextFactory contextFactory, IDateProvider dateProvider,
        ILogger<SignOutEverywhereElseEndpoint> logger, IOptionsSnapshot<AuthenticationOptions> options, RefreshTokenCookieService refreshTokenCookieService)
    {
        this.logger = logger;
        parseRefreshToken = refreshTokenCookieService.ParseCookie;
        getTimestamp = () => dateProvider.Now;
        signOutEverywhereElse = command => SignOutEverywhereElseHandler.Handle(contextFactory, options.Value, command);
        createTokens = authenticationService.CreateTokens;
    }

    [HttpPost("user/sign-out-everywhere-else")]
    public override Task<ActionResult<Tokens>> HandleAsync([FromRoute] RefreshTokenServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from parsedToken in parseRefreshToken(request.Cookie)
            let command = new RefreshTokenCommand(getTimestamp(), parsedToken.UserId, parsedToken.TokenId)
            from user in signOutEverywhereElse(command)
            let tokens = createTokens(user)
            select tokens;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
