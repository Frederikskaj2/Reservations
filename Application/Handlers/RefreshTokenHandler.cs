using LanguageExt;
using static Frederikskaj2.Reservations.Application.Authentication;
using static Frederikskaj2.Reservations.Application.RefreshTokenFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SignOutEverywhereElseHandler
{
    public static EitherAsync<Failure, AuthenticatedUser> Handle(
        IPersistenceContextFactory contextFactory, AuthenticationOptions options, RefreshTokenCommand command) =>
        from context1 in ReadUserContext(DatabaseFunctions.CreateContext(contextFactory), command.UserId)
        let user = context1.Item<User>()
        let refreshToken = user.Security.RefreshTokens.FirstOrDefault(token => token.TokenId == command.TokenId)
        from _1 in FailIfNoMatchingToken(refreshToken, command.TokenId)
        let newRefreshToken = refreshToken with { ExpireAt = GetExpireAt(options, command.Timestamp, refreshToken.IsPersistent) }
        let updatedUser = RemoveOtherTokens(user, newRefreshToken)
        let context2 = context1.UpdateItem(User.GetId(command.UserId), updatedUser)
        from _2 in DatabaseFunctions.WriteContext(context2)
        select CreateAuthenticatedUser(command.Timestamp, updatedUser, newRefreshToken);
}
