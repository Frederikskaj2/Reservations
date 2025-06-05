using System.Collections.Generic;

// ReSharper disable StringLiteralTypo

namespace Frederikskaj2.Reservations.Bank;

public static class AccountNames
{
    public static IEnumerable<AccountName> All { get; } =
    [
        new(PostingAccount.Income, "1201 \u2013 Udlejning af fælleslokale"),
        new(PostingAccount.Bank, "4021 \u2013 Bank"),
        new(PostingAccount.AccountsReceivable, "4401 \u2013 Tilgodehavende fælleslokaler"),
        new(PostingAccount.Deposits, "7601 \u2013 Depositum (IND)"),
        new(PostingAccount.AccountsPayable, "7601 \u2013 Depositum (UD)"),
    ];
}
