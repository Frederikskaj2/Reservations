using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

static class AccountNumberFunctions
{
    public static IPersistenceContext UpdateAccountNumber(
        IPersistenceContext context, Instant timestamp, UserId updatedByUserId, Option<string> accountNumber) =>
        accountNumber.Case switch
        {
            string value => UpdateAccountNumber(context, timestamp, updatedByUserId, value),
            _ => context
        };

    static IPersistenceContext UpdateAccountNumber(IPersistenceContext context, Instant timestamp, UserId updatedByUserId, string accountNumber) =>
        context.UpdateItem<User>(user => user.SetAccountNumber(timestamp, accountNumber, updatedByUserId));
}
