using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class SignOutEndpoint : EndpointBaseAsync.WithRequest<RefreshTokenServerRequest>.WithoutResult
{
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure, ParsedRefreshToken>> parseRefreshToken;
    readonly Func<RefreshTokenCommand, EitherAsync<Failure, Unit>> signOut;

    public SignOutEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<SignOutEndpoint> logger, RefreshTokenCookieService refreshTokenCookieService)
    {
        this.logger = logger;
        parseRefreshToken = refreshTokenCookieService.ParseCookie;
        getTimestamp = () => dateProvider.Now;
        signOut = command => SignOutHandler.Handle(contextFactory, command);
    }

    [HttpPost("user/sign-out")]
    public override Task<ActionResult> HandleAsync([FromRoute] RefreshTokenServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from parsedToken in parseRefreshToken(request.Cookie)
            let command = new RefreshTokenCommand(getTimestamp(), parsedToken.UserId, parsedToken.TokenId)
            from _ in signOut(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
