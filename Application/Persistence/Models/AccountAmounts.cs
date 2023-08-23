using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Frederikskaj2.Reservations.Application;

class AccountAmounts : IEnumerable<(Account Account, Amount Amount)>
{
    public static readonly AccountAmounts Empty = new();

    readonly HashMap<Account, Amount> hashMap;

    AccountAmounts() => hashMap = HashMap<Account, Amount>.Empty;

    AccountAmounts(HashMap<Account, Amount> amounts) => hashMap = amounts;

    // Debit is positive
    public Amount this[Account account] => hashMap.Find(account).Case switch
    {
        Amount amount => amount,
        _ => Amount.Zero
    };

    IEnumerator<(Account, Amount)> IEnumerable<(Account Account, Amount Amount)>.GetEnumerator() => GetEnumeratorForNonZeroAmounts();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorForNonZeroAmounts();

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(GetType().Name);
        stringBuilder.Append(" {");
        stringBuilder.Append(string.Join(',', this.Filter(tuple => tuple.Amount != Amount.Zero).Map(tuple => $" {tuple.Account} = {tuple.Amount}")));
        stringBuilder.Append(" }");
        return stringBuilder.ToString();
    }

    public static AccountAmounts Create(params (Account, Amount)[] items) =>
        ValidateAmounts(new(new(items)));

    public AccountAmounts Apply(AccountAmounts amounts) =>
        ValidateAmounts(amounts.hashMap.Fold(this, folder: (accountAmounts, tuple) => accountAmounts.ApplyAmount(tuple)));

    public AccountAmounts ApplyReverse(AccountAmounts amounts) =>
        ValidateAmounts(amounts.hashMap.Fold(this, folder: (accountAmounts, tuple) => accountAmounts.ApplyAmount((tuple.Item1, Amount.Negate(tuple.Item2)))));

    public AccountAmounts Apply(params (Account, Amount)[] tuples) =>
        tuples.Fold(this, folder: (accountAmounts, tuple) => accountAmounts.ApplyAmount(tuple));

    AccountAmounts ApplyAmount((Account, Amount) tuple)
    {
        var (account, amount) = tuple;
        var updatedHashMap = hashMap.Find(account).Case switch
        {
            Amount existingAmount => hashMap.SetItem(account, existingAmount + amount),
            _ => hashMap.Add(account, amount)
        };
        return new(updatedHashMap);
    }

    static AccountAmounts ValidateAmounts(AccountAmounts amounts)
    {
#if DEBUG
        Validate(() => amounts.hashMap.Sum(tuple => tuple.Value) == Amount.Zero);
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

    IEnumerator<(Account Key, Amount Value)> GetEnumeratorForNonZeroAmounts() =>
        hashMap.Filter(tuple => tuple.Item2 != Amount.Zero).GetEnumerator();
}
