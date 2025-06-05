using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Users;

static class DeleteMyUser
{
    public static Either<Failure<Unit>, DeleteMyUserOutput> DeleteMyUserCore(DeleteMyUserInput input) =>
        !input.User.Flags.HasFlag(UserFlags.IsPendingDelete) && input.User.Roles.HasFlag(Roles.UserAdministration)
            ? Failure.New(HttpStatusCode.UnprocessableEntity, "User in role user administration cannot delete self.")
            : new DeleteMyUserOutput(
                input.User.UpdateIsPendingDelete(input.Command.Timestamp, isPendingDelete: true, input.User.UserId),
                CanDeleteUser(input.User) ? DeleteUserStatus.Confirmed : DeleteUserStatus.Pending);

    static bool CanDeleteUser(User user) =>
        user.Orders.IsEmpty && user.Balance() == Amount.Zero;
}
