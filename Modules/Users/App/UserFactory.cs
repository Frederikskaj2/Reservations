namespace Frederikskaj2.Reservations.Users;

public static class UserFactory
{
    public static UserDto CreateUser(User user) =>
        new(
            UserIdentityFactory.CreateUserIdentity(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.Flags.HasFlag(UserFlags.IsDeleted),
            user.Orders,
            user.HistoryOrders);
}
