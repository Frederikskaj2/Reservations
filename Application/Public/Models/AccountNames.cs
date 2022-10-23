using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

public static class AccountNames
{
    static readonly IEnumerable<AccountName> accountNames = new AccountName[]
    {
        new(PostingAccount.Income, "1201 - Udlejning af fælleslokale"),
        new(PostingAccount.Bank, "4021 - Bank"),
        new(PostingAccount.AccountsReceivable, "4401 – Tilgodehavende fælleslokaler"),
        new(PostingAccount.Deposits, "7601- Depositum (IND)"),
        new(PostingAccount.AccountsPayable, "7601 Depositum (UD)")
    };

    public static IEnumerable<AccountName> GetAll() => accountNames;
}
