using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class UpdateRefreshToken
{
    public static Either<Failure<Unit>, UpdateRefreshTokenOutput> UpdateRefreshTokenCore(AuthenticationOptions options, UpdateRefreshTokenInput input) =>
        from refreshToken in GetRefreshToken(input.Command.TokenId, input.User)
        from _ in EnsureTokenIsNotExpired(input.Command.Timestamp, refreshToken)
        let newRefreshToken = refreshToken with { ExpireAt = Authentication.GetExpireAt(options, input.Command.Timestamp, refreshToken.IsPersistent) }
        let refreshTokens = CreateRefreshTokens(input.Command.Timestamp, input.Command.TokenId, input.User, newRefreshToken)
        select new UpdateRefreshTokenOutput(input.User with { Security = input.User.Security with { RefreshTokens = refreshTokens } }, newRefreshToken);

    static Either<Failure<Unit>, RefreshToken> GetRefreshToken(TokenId tokenId, User user) =>
        user.Security.RefreshTokens.Filter(refreshToken => refreshToken.TokenId == tokenId).HeadOrNone().Case switch
        {
            RefreshToken refreshToken => refreshToken,
            _ => Failure.New(HttpStatusCode.Forbidden, $"Token {tokenId} does not exist."),
        };

    static Either<Failure<Unit>, Unit> EnsureTokenIsNotExpired(Instant timestamp, RefreshToken refreshToken) =>
        refreshToken.ExpireAt > timestamp
            ? unit
            : Failure.New(HttpStatusCode.Forbidden, $"Token {refreshToken.TokenId} expired at {refreshToken.ExpireAt}.");

    static Seq<RefreshToken> CreateRefreshTokens(Instant timestamp, TokenId tokenId, User user, RefreshToken newRefreshToken) =>
        user.Security.RefreshTokens
            .Filter(token => token.TokenId != tokenId && timestamp <= token.ExpireAt)
            .Add(newRefreshToken);
}
