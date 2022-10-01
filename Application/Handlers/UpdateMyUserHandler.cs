using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateMyUserHandler
{
    public static EitherAsync<Failure, MyUser> Handle(IPersistenceContextFactory contextFactory, UpdateMyUserCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        let context2 = context1.UpdateItem<User>(user => UpdateUser(user, command))
        from _ in WriteContext(context2)
        select CreateMyUser(context2.Item<User>());
}
