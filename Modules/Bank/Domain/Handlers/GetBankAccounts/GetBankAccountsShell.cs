using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Bank;

public static class GetBankAccountsShell
{
    public static EitherAsync<Failure<Unit>, Seq<string>> GetBankAccounts(CancellationToken _) =>
        EitherAsync<Failure<Unit>, Seq<string>>.Right(BankAccounts.All.Map(bankAccount => bankAccount.Name));
}
