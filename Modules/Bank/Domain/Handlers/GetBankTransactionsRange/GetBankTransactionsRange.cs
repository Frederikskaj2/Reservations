using LanguageExt;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class GetBankTransactionsRange
{
    public static GetBankTransactionsRangeOutput GetBankTransactionsRangeCore(GetBankTransactionsRangeInput input) =>
        new(GetDateRange(input.EarliestTransaction, input.LatestTransaction), input.Import.Map(import => import.StartDate));

    static Option<DateRange> GetDateRange(Option<BankTransaction> earliestTransaction, Option<BankTransaction> latestTransaction) =>
        (earliestTransaction.Case, latestTransaction.Case) switch
        {
            (BankTransaction earliest, BankTransaction latest) => new DateRange(earliest.Date, latest.Date),
            _ => None,
        };
}
