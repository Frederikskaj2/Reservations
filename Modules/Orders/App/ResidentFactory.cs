using Frederikskaj2.Reservations.Users;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Orders;

static class ResidentFactory
{
    public static ResidentDto CreateResident(User user) =>
        new(
            CreateUserIdentity(user),
            FromUserId(user.UserId),
            -(user.Accounts[Account.AccountsReceivable] + user.Accounts[Account.AccountsPayable]));
}
