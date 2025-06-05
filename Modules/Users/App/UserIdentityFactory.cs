namespace Frederikskaj2.Reservations.Users;

public static class UserIdentityFactory
{
    public static UserIdentityDto CreateUserIdentity(User user) =>
        new(user.UserId, user.Email(), user.FullName, user.Phone, user.ApartmentId.ToNullable());

    public static UserIdentityDto CreateUserIdentity(UserExcerpt user) =>
        new(user.UserId, user.Email(), user.FullName, user.Phone, user.ApartmentId.ToNullable());
}
