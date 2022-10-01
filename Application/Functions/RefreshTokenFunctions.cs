using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class RefreshTokenFunctions
{
    public static EitherAsync<Failure, IPersistenceContext> ReadUserContext(IPersistenceContext context, UserId userId) =>
        HandleReadError(context.ReadItem<User>(User.GetId(userId)));

    static EitherAsync<Failure, IPersistenceContext> HandleReadError(EitherAsync<HttpStatusCode, IPersistenceContext> either) =>
        either.MapLeft(MapReadError);

    static Failure MapReadError(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => Failure.New(HttpStatusCode.Forbidden),
            _ => Failure.New(HttpStatusCode.ServiceUnavailable, $"Database read error {status}.")
        };

    public static EitherAsync<Failure, Unit> FailIfNoMatchingToken(RefreshToken? refreshToken, TokenId tokenId) =>
        refreshToken is not null ? unit : Failure.New(HttpStatusCode.Forbidden, $"Token {tokenId} does not exist.");

    public static EitherAsync<Failure, Unit> FailIfTokenIsExpired(RefreshToken refreshToken, Instant timestamp) =>
        refreshToken.ExpireAt > timestamp ? unit : Failure.New(HttpStatusCode.Forbidden, $"Token {refreshToken.TokenId} expired at {refreshToken.ExpireAt}.");

    public static Seq<RefreshToken> CreateRefreshTokens(Instant timestamp, TokenId tokenId, User user, RefreshToken newRefreshToken) =>
        user.Security.RefreshTokens
            .Filter(token => token.TokenId != tokenId && timestamp <= token.ExpireAt)
            .Add(newRefreshToken);

    public static User RemoveToken(User user, Instant timestamp, TokenId tokenId) =>
        user with
        {
            Security = user.Security with
            {
                RefreshTokens = user.Security.RefreshTokens.Where(token => token.TokenId != tokenId && timestamp <= token.ExpireAt).ToSeq()
            }
        };

    public static User RemoveOtherTokens(User user, RefreshToken token) =>
        user with { Security = user.Security with { RefreshTokens = token.Cons() } };
}
