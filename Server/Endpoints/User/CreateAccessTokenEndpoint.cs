using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public class CreateAccessTokenEndpoint : EndpointBaseAsync.WithRequest<RefreshTokenServerRequest>.WithActionResult<Tokens>
{
    readonly Func<AuthenticatedUser, Tokens> createTokens;
    readonly Func<AuthenticatedUser, Cookie> createRefreshTokenCookie;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure, ParsedRefreshToken>> parseRefreshTokenCookie;
    readonly Func<RefreshTokenCommand, EitherAsync<Failure, AuthenticatedUser>> updateRefreshToken;

    public CreateAccessTokenEndpoint(
        AuthenticationService authenticationService, IPersistenceContextFactory contextFactory, IDateProvider dateProvider,
        ILogger<CreateAccessTokenEndpoint> logger, IOptionsSnapshot<AuthenticationOptions> options, RefreshTokenCookieService refreshTokenCookieService)
    {
        this.logger = logger;
        parseRefreshTokenCookie = refreshTokenCookieService.ParseCookie;
        getTimestamp = () => dateProvider.Now;
        updateRefreshToken = command => UpdateRefreshTokenHandler.Handle(contextFactory, options.Value, command);
        createTokens = authenticationService.CreateTokens;
        createRefreshTokenCookie = user => refreshTokenCookieService.CreateCookie(user.UserId, user.RefreshToken.TokenId, user.RefreshToken.IsPersistent);
    }

    [HttpPost("user/create-access-token")]
    public override Task<ActionResult<Tokens>> HandleAsync([FromRoute] RefreshTokenServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from parsedRefreshToken in parseRefreshTokenCookie(request.Cookie)
            let command = new RefreshTokenCommand(getTimestamp(), parsedRefreshToken.UserId, parsedRefreshToken.TokenId)
            from user in updateRefreshToken(command)
            let tokens = createTokens(user)
            let refreshTokenCookie = createRefreshTokenCookie(user)
            select new WithCookies<Tokens>(tokens, new[] { refreshTokenCookie });
        return either.ToResultWithCookieAsync(logger, HttpContext, true);
    }
}
