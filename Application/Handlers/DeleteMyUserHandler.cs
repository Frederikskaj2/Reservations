using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class DeleteMyUserHandler
{
    public static EitherAsync<Failure, DeleteUserResponse> Handle(IPersistenceContextFactory contextFactory, IEmailService emailService, DeleteMyUserCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        from user in TryUpdateIsPendingDelete(context1.Item<User>(), command)
        let context2 = context1.UpdateItem(User.GetId(user.UserId), user)
        from context3 in TryDeleteUser(emailService, context2, command.Timestamp, command.UserId)
        from _2 in WriteContext(context3)
        let result = context3.Item<User>().Flags.HasFlag(UserFlags.IsDeleted) ? DeleteUserResult.Success : DeleteUserResult.IsPendingDelete
        select new DeleteUserResponse(result);
}
