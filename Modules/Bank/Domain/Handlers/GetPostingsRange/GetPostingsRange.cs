using Frederikskaj2.Reservations.Orders;
using LanguageExt;
using NodaTime;
using System.Diagnostics;

namespace Frederikskaj2.Reservations.Bank;

static class GetPostingsRange
{
    public static GetPostingsRangeOutput GetPostingsRangeCore(GetPostingsRangeInput input) =>
        (input.EarliestTransaction.Case, input.LatestTransaction.Case) switch
        {
            (Transaction earliest, Transaction latest) => new(GetPostingsRangeFromExistingTransactions(earliest, latest)),
            (OptionNone, OptionNone) => new(CreatePostingsRangeForThisMonth(input.Query)),
            _ => throw new UnreachableException(),
        };

    static MonthRange GetPostingsRangeFromExistingTransactions(Transaction earliestTransaction, Transaction latestTransaction) =>
        new(GetMonthStart(earliestTransaction.Date), GetMonthStart(latestTransaction.Date));

    static MonthRange CreatePostingsRangeForThisMonth(GetPostingsRangeQuery query) =>
        CreatePostingsRangeForMonth(GetMonthStart(query.Today));

    static MonthRange CreatePostingsRangeForMonth(LocalDate monthStart) =>
        new(monthStart, monthStart);

    static LocalDate GetMonthStart(LocalDate date) =>
        date.PlusDays(-(date.Day - 1));
}
