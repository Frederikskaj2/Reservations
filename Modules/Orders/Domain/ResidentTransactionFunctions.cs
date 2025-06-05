using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Orders;

public static class ResidentTransactionFunctions
{
    public static User AddTransaction(this User user, Transaction transaction) =>
        user with { Accounts = ValidateResidentAccounts(user.Accounts.Apply(transaction.Amounts)) };

    static AccountAmounts ValidateResidentAccounts(AccountAmounts amounts)
    {
#if DEBUG
        Validate(() => amounts[Account.AccountsReceivable] == Amount.Zero || amounts[Account.AccountsPayable] == Amount.Zero);
#endif
        return amounts;
    }

    [Conditional("DEBUG")]
    static void Validate(Expression<Func<bool>> predicate)
    {
        var func = predicate.Compile();
        if (!func())
            throw new InternalValidationException(predicate.Body.ToString());
    }
}
