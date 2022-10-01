using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Application.UserFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateUserHandler
{
    public static EitherAsync<Failure, UpdateUserResponse> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, UpdateUserCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        from user1 in TryUpdateRoles(context1.Item<User>(), command)
        from user2 in TryUpdateIsPendingDelete(user1, command)
        let user3 = UpdateUser(user2, command)
        let context2 = context1.UpdateItem(User.GetId(command.UserId), user3)
        from context3 in TryDeleteUser(emailService, context2, command.Timestamp, command.AdministratorUserId)
        from _3 in WriteContext(context3)
        let user = context3.Item<User>()
        let userIds = toHashSet(user.Audits.Where(audit => audit.UserId.HasValue).Map(audit => audit.UserId!.Value))
        from userFullNames in ReadUserFullNames(CreateContext(contextFactory), userIds)
        let hashMap = toHashMap(userFullNames.Map(u => (u.UserId, u.FullName)))
        let result = user.Flags.HasFlag(UserFlags.IsDeleted) ? UpdateUserResult.UserWasDeleted : default
        select new UpdateUserResponse(result, result == default ? CreateUserDetails(user, hashMap) : null);
}
