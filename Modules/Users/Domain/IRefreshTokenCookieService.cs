using Frederikskaj2.Reservations.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

public interface IRefreshTokenCookieService
{
    Cookie CreateCookie(UserId userId, TokenId tokenId, bool isPersistent);
    EitherAsync<Failure<T>, ParsedRefreshToken> ParseCookie<T>(string? cookie, T error = default) where T : struct;
}
