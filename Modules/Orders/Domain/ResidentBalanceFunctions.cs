using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Frederikskaj2.Reservations.Orders;

static class ResidentBalanceFunctions
{
    public static User UpdateResidentBalance(User user, Transaction transaction) =>
        user with { Accounts = ValidateResidentAccounts(TryEqualizeAccountsReceivableAndAccountsPayable(user.Accounts.Apply(transaction.Amounts))) };

    public static AccountAmounts TryEqualizeAccountsReceivableAndAccountsPayable(AccountAmounts amounts) =>
        amounts[Account.AccountsReceivable] != Amount.Zero && amounts[Account.AccountsPayable] != Amount.Zero
            ? EqualizeAccountsReceivableAndAccountsPayable(amounts)
            : amounts;

    static AccountAmounts EqualizeAccountsReceivableAndAccountsPayable(AccountAmounts amounts) =>
        amounts[Account.AccountsReceivable] >= -amounts[Account.AccountsPayable]
            ? DeductAccountsPayableFromAccountsReceivable(amounts)
            : DeductAccountsReceivableFromAccountsPayable(amounts);

    static AccountAmounts DeductAccountsPayableFromAccountsReceivable(AccountAmounts amounts) =>
        amounts.Apply((Account.AccountsReceivable, amounts[Account.AccountsPayable]), (Account.AccountsPayable, -amounts[Account.AccountsPayable]));

    static AccountAmounts DeductAccountsReceivableFromAccountsPayable(AccountAmounts amounts) =>
        amounts.Apply((Account.AccountsPayable, amounts[Account.AccountsReceivable]), (Account.AccountsReceivable, -amounts[Account.AccountsReceivable]));

    public static AccountAmounts ValidateResidentAccounts(AccountAmounts amounts)
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
