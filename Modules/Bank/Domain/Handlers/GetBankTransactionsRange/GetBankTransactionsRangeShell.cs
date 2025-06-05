using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.GetBankTransactionsRange;

namespace Frederikskaj2.Reservations.Bank;

public static class GetBankTransactionsRangeShell
{
    static readonly IProjectedQuery<BankTransaction> getEarliestTransactionQuery =
        QueryFactory.Query<BankTransaction>().Top(1).OrderBy(transaction => transaction.Date).Project();

    static readonly IProjectedQuery<BankTransaction> getLatestTransactionQuery =
        QueryFactory.Query<BankTransaction>().Top(1).OrderByDescending(transaction => transaction.Date).Project();

    public static EitherAsync<Failure<Unit>, BankTransactionsRange> GetBankTransactionsRange(IEntityReader reader, CancellationToken cancellationToken) =>
        from earliestTransaction in ReadEarliestTransaction(reader, cancellationToken)
        from latestTransaction in ReadLatestTransaction(reader, cancellationToken)
        from import in ReadImport(reader, cancellationToken)
        let output = GetBankTransactionsRangeCore(new(earliestTransaction, latestTransaction, import))
        select new BankTransactionsRange(output.DateRange, output.LatestImportStartDate);

    static EitherAsync<Failure<Unit>, Option<BankTransaction>> ReadEarliestTransaction(IEntityReader reader, CancellationToken cancellationToken) =>
        from transactions in reader.Query(getEarliestTransactionQuery, cancellationToken).MapReadError()
        select transactions.HeadOrNone();

    static EitherAsync<Failure<Unit>, Option<BankTransaction>> ReadLatestTransaction(IEntityReader reader, CancellationToken cancellationToken) =>
        from transactions in reader.Query(getLatestTransactionQuery, cancellationToken).MapReadError()
        select transactions.HeadOrNone();

    static EitherAsync<Failure<Unit>, Option<Import>> ReadImport(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption();
}
