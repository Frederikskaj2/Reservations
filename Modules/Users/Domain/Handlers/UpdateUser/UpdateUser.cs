using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Users;

static class UpdateUser
{
    public static Either<Failure<Unit>, UpdateUserOutput> UpdateUserCore(UpdateUserInput input) =>
        from user1 in UpdateRoles(input.Command, input.User)
        from user2 in UpdateIsPendingDelete(input.Command, user1)
        select new UpdateUserOutput(UpdateUserFullNameAndPhone(input.Command, user2));

    static Either<Failure<Unit>, User> UpdateRoles(UpdateUserCommand command, User user) =>
        user.Roles != command.Roles && user.Roles.HasFlag(Roles.UserAdministration) && !command.Roles.HasFlag(Roles.UserAdministration) &&
        user.UserId == command.AdministratorId
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User cannot remove user administration role from self.")
            : user.UpdateRoles(command.Timestamp, command.Roles, command.AdministratorId);

    static Either<Failure<Unit>, User> UpdateIsPendingDelete(UpdateUserCommand command, User user) =>
        !user.Flags.HasFlag(UserFlags.IsPendingDelete) && command.IsPendingDelete && user.UserId == command.AdministratorId
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User in role user administration cannot delete self.")
            : user.UpdateIsPendingDelete(command.Timestamp, command.IsPendingDelete, command.AdministratorId);

    static User UpdateUserFullNameAndPhone(UpdateUserCommand command, User user) =>
        user
            .UpdateFullName(command.Timestamp, command.FullName, command.AdministratorId)
            .UpdatePhone(command.Timestamp, command.Phone, command.AdministratorId);
}
