using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

static class AccountNumberFunctions
{
    public static User UpdateAccountNumber(User user, Instant timestamp, UserId updatedByUserId, Option<AccountNumber> accountNumber) =>
        accountNumber.Case switch
        {
            AccountNumber value => user.SetAccountNumber(timestamp, value, updatedByUserId),
            _ => user,
        };
}
