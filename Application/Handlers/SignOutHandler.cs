using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.RefreshTokenFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class SignOutHandler
{
    public static EitherAsync<Failure, Unit> Handle(IPersistenceContextFactory contextFactory, RefreshTokenCommand command) =>
        from context1 in RefreshTokenFunctions.ReadUserContext(CreateContext(contextFactory), command.UserId)
        let user = context1.Item<User>()
        let updatedUser = RemoveToken(user, command.Timestamp, command.TokenId)
        let context2 = context1.UpdateItem(User.GetId(command.UserId), updatedUser)
        from _ in WriteContext(context2)
        select unit;
}
