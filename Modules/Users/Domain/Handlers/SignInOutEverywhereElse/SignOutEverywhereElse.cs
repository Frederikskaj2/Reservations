using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Users;

static class SignOutEverywhereElse
{
    public static Either<Failure<Unit>, SignOutEverywhereElseOutput> SignOutEverywhereElseCore(
        AuthenticationOptions options, SignOutEverywhereElseInput input) =>
        from refreshToken in GetToken(input.Command.TokenId, input.User)
        let newRefreshToken = refreshToken with { ExpireAt = Authentication.GetExpireAt(options, input.Command.Timestamp, refreshToken.IsPersistent) }
        select new SignOutEverywhereElseOutput(RemoveOtherTokens(input.User, newRefreshToken), newRefreshToken);

    static Either<Failure<Unit>, RefreshToken> GetToken(TokenId tokenId, User user) =>
        user.Security.RefreshTokens.Filter(refreshToken => refreshToken.TokenId == tokenId).HeadOrNone().Case switch
        {
            RefreshToken token => token,
            _ => Failure.New(HttpStatusCode.Forbidden, $"Token {tokenId} does not exist."),
        };

    static User RemoveOtherTokens(User user, RefreshToken refreshToken) =>
        user with { Security = user.Security with { RefreshTokens = refreshToken.Cons() } };
}
