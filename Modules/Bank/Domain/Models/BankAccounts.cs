using LanguageExt;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class BankAccounts
{
    public static Seq<BankAccount> All { get; } = new BankAccount[]
    {
        new(BankAccountId.Dedicated, "3000-4816068614"),
        new(BankAccountId.Shared, "9444-12501110"),
    }.ToSeq();

    static readonly HashMap<string, BankAccountId> names = toHashMap(All.Map(bankAccount => (bankAccount.Name, BankAccount: bankAccount.BankAccountId)));

    public static Option<BankAccountId> GetBankAccountId(string name) => names.Find(name);
}
