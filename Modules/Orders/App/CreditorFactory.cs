using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Orders;

static class CreditorFactory
{
    public static CreditorDto CreateCreditor(User user) =>
        CreateCreditor(user, -user.Accounts[Account.AccountsPayable]);

    static CreditorDto CreateCreditor(User user, Amount accountsPayable) =>
        new(
            CreateUserIdentity(user),
            GetPaymentInformation(user, accountsPayable));

    static PaymentInformation GetPaymentInformation(User user, Amount accountsPayable) =>
        new(
            PaymentIdEncoder.FromUserId(user.UserId),
            accountsPayable,
            GetAccountNumberAllowingNullWhenValueIsMissing(user));

    // There have been cases of mistakes related to pay-outs where the resident
    // had their account number removed but became a creditor after the problem was
    // resolved. It's better to allow the missing account number to be passed to
    // the client instead of returning internal server error on the creditors API.
    static string GetAccountNumberAllowingNullWhenValueIsMissing(User user) => user.AccountNumber.ToNullableReference()!;
}
