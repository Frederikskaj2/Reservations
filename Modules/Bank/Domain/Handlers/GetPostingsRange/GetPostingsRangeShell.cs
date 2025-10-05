using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.GetPostingsRange;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPostingsRangeShell
{
    static readonly IProjectedQuery<Transaction> getEarliestTransactionQuery =
        Query<Transaction>().Top(1).OrderBy(transaction => transaction.Timestamp).Project();

    static readonly IProjectedQuery<Transaction> getLatestTransactionQuery =
        Query<Transaction>().Top(1).OrderByDescending(transaction => transaction.Timestamp).Project();

    public static EitherAsync<Failure<Unit>, MonthRange> GetPostingsRange(
        IEntityReader reader, GetPostingsRangeQuery query, CancellationToken cancellationToken) =>
        from earliestTransaction in ReadEarliestTransaction(reader, cancellationToken)
        from latestTransaction in ReadLatestTransaction(reader, cancellationToken)
        let output = GetPostingsRangeCore(new(query, earliestTransaction, latestTransaction))
        select output.MonthRange;

    static EitherAsync<Failure<Unit>, Option<Transaction>> ReadEarliestTransaction(IEntityReader reader, CancellationToken cancellationToken) =>
        from transactions in reader.Query(getEarliestTransactionQuery, cancellationToken).MapReadError()
        select transactions.HeadOrNone();

    static EitherAsync<Failure<Unit>, Option<Transaction>> ReadLatestTransaction(IEntityReader reader, CancellationToken cancellationToken) =>
        from transactions in reader.Query(getLatestTransactionQuery, cancellationToken).MapReadError()
        select transactions.HeadOrNone();
}
