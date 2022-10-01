using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public class RefreshTokenCookieService
{
    const string cookieName = "User";
    readonly ILogger logger;
    readonly AuthenticationOptions options;
    readonly CookieValueSerializer serializer;

    public RefreshTokenCookieService(
        ILogger<RefreshTokenCookieService> logger, IOptionsSnapshot<AuthenticationOptions> options, CookieValueSerializer serializer)
    {
        this.logger = logger;
        this.serializer = serializer;
        this.options = options.Value;
    }

    public Cookie CreateCookie(UserId userId, TokenId tokenId, bool isPersistent)
    {
        var cookie = new RefreshTokenCookie(userId, tokenId);
        logger.LogTrace("Creating refresh token cookie {Cookie}", cookie);
        return CookieFactory.CreateCookie(
            cookieName,
            serializer.Serialize(cookie),
            isPersistent ? options.LongRefreshTokenLifetime : options.ShortRefreshTokenLifetime);
    }

    public EitherAsync<Failure, ParsedRefreshToken> ParseCookie(string? cookie) =>
        TryParseCookie(cookie).Case switch
        {
            ParsedRefreshToken refreshTokenCookie => refreshTokenCookie,
            _ => Failure.New(HttpStatusCode.BadRequest, "Invalid refresh token cookie.")
        };

    public EitherAsync<Failure<T>, ParsedRefreshToken> ParseCookie<T>(string? cookie, T error) where T : struct, IConvertible =>
        TryParseCookie(cookie).Case switch
        {
            ParsedRefreshToken refreshTokenCookie => refreshTokenCookie,
            _ => Failure.New(HttpStatusCode.BadRequest, error, "Invalid refresh token cookie.")
        };

    Option<ParsedRefreshToken> TryParseCookie(string? cookie)
    {
        var parsedRefreshToken = GetRefreshTokenCookieOption(serializer.Deserialize<RefreshTokenCookie>(CookieFactory.ParseCookie(cookieName, cookie)));
        parsedRefreshToken.Match(
            Some: refreshToken => logger.LogTrace("Received refresh token {RefreshToken} from cookie", refreshToken),
            None: () => logger.LogTrace("Received no refresh token cookie"));
        return parsedRefreshToken;
    }

    static Option<ParsedRefreshToken> GetRefreshTokenCookieOption(RefreshTokenCookie? cookie) =>
        cookie is { UserId: { }, TokenId: { } } ? new ParsedRefreshToken(cookie.UserId.Value, cookie.TokenId.Value) : None;
}
