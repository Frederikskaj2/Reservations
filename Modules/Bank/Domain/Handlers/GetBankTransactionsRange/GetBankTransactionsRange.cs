using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class GetBankTransactionsRange
{
    public static GetBankTransactionsRangeOutput GetBankTransactionsRangeCore(GetBankTransactionsRangeInput input) =>
        new(GetDateRange(input.EarliestTransaction, input.LatestTransaction), GetLatestImportStartDate(input.Query.BankAccountId, input.Import));

    static Option<DateRange> GetDateRange(Option<BankTransaction> earliestTransaction, Option<BankTransaction> latestTransaction) =>
        (earliestTransaction.Case, latestTransaction.Case) switch
        {
            (BankTransaction earliest, BankTransaction latest) => new DateRange(earliest.Date, latest.Date),
            _ => None,
        };

    static Option<LocalDate> GetLatestImportStartDate(BankAccountId bankAccountId, Option<Import> importOption) =>
        importOption.Case switch
        {
            Import import => import.BankAccounts
                .Find(bankAccountImport => bankAccountImport.BankAccountId == bankAccountId)
                .Map(bankAccountImport => bankAccountImport.StartDate),
            _ => None,
        };
}
