using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.GetBankTransactionsRange;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Bank;

public static class GetBankTransactionsRangeShell
{
    public static EitherAsync<Failure<Unit>, BankTransactionsRange> GetBankTransactionsRange(
        IEntityReader reader, GetBankTransactionsRangeQuery query, CancellationToken cancellationToken) =>
        from earliestTransaction in ReadEarliestTransaction(reader, query, cancellationToken)
        from latestTransaction in ReadLatestTransaction(reader, query, cancellationToken)
        from import in ReadImport(reader, cancellationToken)
        let output = GetBankTransactionsRangeCore(new(query, earliestTransaction, latestTransaction, import))
        select new BankTransactionsRange(output.DateRange, output.LatestImportStartDate);

    static EitherAsync<Failure<Unit>, Option<BankTransaction>> ReadEarliestTransaction(
        IEntityReader reader, GetBankTransactionsRangeQuery query, CancellationToken cancellationToken) =>
        from transactions in reader.Query(GetEarliestTransactionQuery(query.BankAccountId), cancellationToken).MapReadError()
        select transactions.HeadOrNone();

    static IProjectedQuery<BankTransaction> GetEarliestTransactionQuery(BankAccountId bankAccountId) =>
        Query<BankTransaction>()
            .Where(bankAccount => bankAccount.BankAccountId == bankAccountId)
            .Top(1)
            .OrderBy(transaction => transaction.Date)
            .Project();

    static EitherAsync<Failure<Unit>, Option<BankTransaction>> ReadLatestTransaction(
        IEntityReader reader, GetBankTransactionsRangeQuery query, CancellationToken cancellationToken) =>
        from transactions in reader.Query(GetLatestTransactionQuery(query.BankAccountId), cancellationToken).MapReadError()
        select transactions.HeadOrNone();

    static IProjectedQuery<BankTransaction> GetLatestTransactionQuery(BankAccountId bankAccountId) =>
        Query<BankTransaction>()
            .Where(bankAccount => bankAccount.BankAccountId == bankAccountId)
            .Top(1)
            .OrderByDescending(transaction => transaction.Date)
            .Project();

    static EitherAsync<Failure<Unit>, Option<Import>> ReadImport(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption();
}
