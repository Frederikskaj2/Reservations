using Frederikskaj2.Reservations.Core;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Users;

public static class MyUserResponseFactory
{
    public static GetMyUserResponse CreateMyUserResponse(User user) =>
        new(
            CreateUserIdentity(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.EmailSubscriptions,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.AccountNumber.ToNullableReference());
}
