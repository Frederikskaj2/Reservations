using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public static class AccountNames
{
    public static IEnumerable<AccountName> All { get; } =
    [
        new(PostingAccount.Income, "Indtægter"),
        new(PostingAccount.BankShared, "Bank (gammel)"),
        new(PostingAccount.BankDedicated, "Bank (ny)"),
        new(PostingAccount.AccountsReceivable, "Tilgodehavende hos beboerne"),
        new(PostingAccount.Deposits, "Depositum"),
        new(PostingAccount.AccountsPayable, "Gæld til beboerne"),
    ];
}
