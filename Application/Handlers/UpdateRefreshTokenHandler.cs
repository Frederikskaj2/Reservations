using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.RefreshTokenFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateRefreshTokenHandler
{
    public static EitherAsync<Failure, AuthenticatedUser> Handle(
        IPersistenceContextFactory contextFactory, AuthenticationOptions options, RefreshTokenCommand command) =>
        from context1 in RefreshTokenFunctions.ReadUserContext(CreateContext(contextFactory), command.UserId)
        let user = context1.Item<User>()
        let refreshToken = user.Security.RefreshTokens.FirstOrDefault(token => token.TokenId == command.TokenId)
        from _1 in FailIfNoMatchingToken(refreshToken, command.TokenId)
        from _2 in FailIfTokenIsExpired(refreshToken, command.Timestamp)
        let newRefreshToken = refreshToken with { ExpireAt = Authentication.GetExpireAt(options, command.Timestamp, refreshToken.IsPersistent) }
        let refreshTokens = CreateRefreshTokens(command.Timestamp, command.TokenId, user, newRefreshToken)
        let updatedUser = user with { Security = user.Security with { RefreshTokens = refreshTokens } }
        let context2 = context1.UpdateItem(User.GetId(command.UserId), updatedUser)
        from _3 in WriteContext(context2)
        select CreateAuthenticatedUser(command.Timestamp, updatedUser, newRefreshToken);
}
