using Frederikskaj2.Reservations.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class RefreshTokenCookieService(ILogger<RefreshTokenCookieService> logger, IOptionsSnapshot<AuthenticationOptions> options, CookieValueSerializer serializer)
    : IRefreshTokenCookieService
{
    const string cookieName = "User";
    readonly AuthenticationOptions options = options.Value;

    public Cookie CreateCookie(UserId userId, TokenId tokenId, bool isPersistent)
    {
        var cookie = new RefreshTokenCookie(userId, tokenId);
        logger.LogTrace("Creating refresh token cookie {Cookie}", cookie);
        return CookieProvider.CreateCookie(
            cookieName,
            serializer.Serialize(cookie),
            isPersistent ? options.LongRefreshTokenLifetime : options.ShortRefreshTokenLifetime);
    }

    public EitherAsync<Failure<T>, ParsedRefreshToken> ParseCookie<T>(string? cookie, T error = default) where T : struct =>
        TryParseCookie(cookie).Case switch
        {
            ParsedRefreshToken refreshTokenCookie => refreshTokenCookie,
            _ => Failure.New(HttpStatusCode.BadRequest, error, "Invalid refresh token cookie."),
        };

    Option<ParsedRefreshToken> TryParseCookie(string? cookie)
    {
        var parsedRefreshToken = GetRefreshTokenCookieOption(serializer.Deserialize<RefreshTokenCookie>(CookieProvider.ParseCookie(cookieName, cookie)));
        parsedRefreshToken.Match(
            refreshToken => logger.LogTrace("Received refresh token {RefreshToken} from cookie", refreshToken),
            () => logger.LogTrace("Refresh token cookie is missing or invalid"));
        return parsedRefreshToken;
    }

    static Option<ParsedRefreshToken> GetRefreshTokenCookieOption(RefreshTokenCookie? cookie) =>
        cookie is { UserId: not null, TokenId: not null } ? new ParsedRefreshToken(cookie.UserId.Value, cookie.TokenId.Value) : None;
}
