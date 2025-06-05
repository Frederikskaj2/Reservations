using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System;
using System.Linq.Expressions;
using System.Threading;

namespace Frederikskaj2.Reservations.Bank;

public static class GetBankTransactionsShell
{
    static readonly Expression<Func<BankTransaction, bool>> falsePredicate = _ => false;
    static readonly Expression<Func<BankTransaction, bool>> truePredicate = _ => true;

    public static EitherAsync<Failure<Unit>, Seq<BankTransaction>> GetBankTransactions(
        IEntityReader reader, GetBankTransactionsQuery query, CancellationToken cancellationToken) =>
        from transactions in reader.Query(GetQuery(query), cancellationToken).MapReadError()
        select transactions;

    static IProjectedQuery<BankTransaction> GetQuery(GetBankTransactionsQuery query) =>
        QueryFactory.Query<BankTransaction>().Where(GetFilterPredicate(query)).OrderBy(transaction => transaction.BankTransactionId).Project();

    static Expression<Func<BankTransaction, bool>> GetFilterPredicate(GetBankTransactionsQuery query) =>
        truePredicate
            .ConditionallyIncludeStartDate(query)
            .ConditionallyIncludeEndDate(query)
            .And(
                falsePredicate
                    .ConditionallyIncludeUnknown(query)
                    .ConditionallyIncludeIgnored(query)
                    .ConditionallyIncludeReconciled(query));

    static Expression<Func<BankTransaction, bool>> ConditionallyIncludeStartDate(
        this Expression<Func<BankTransaction, bool>> predicate, GetBankTransactionsQuery query) =>
        query.StartDate.Case switch
        {
            LocalDate startDate => predicate.And(transaction => transaction.Date >= startDate),
            _ => predicate,
        };

    static Expression<Func<BankTransaction, bool>> ConditionallyIncludeEndDate(
        this Expression<Func<BankTransaction, bool>> predicate, GetBankTransactionsQuery query) =>
        query.EndDate.Case switch
        {
            LocalDate endDate => predicate.And(transaction => transaction.Date < endDate),
            _ => predicate,
        };

    static Expression<Func<BankTransaction, bool>> ConditionallyIncludeUnknown(
        this Expression<Func<BankTransaction, bool>> predicate, GetBankTransactionsQuery query) =>
        query.IncludeUnknown ? predicate.IncludeStatus(BankTransactionStatus.Unknown) : predicate;

    static Expression<Func<BankTransaction, bool>> ConditionallyIncludeIgnored(
        this Expression<Func<BankTransaction, bool>> predicate, GetBankTransactionsQuery query) =>
        query.IncludeIgnored ? predicate.IncludeStatus(BankTransactionStatus.Ignored) : predicate;

    static Expression<Func<BankTransaction, bool>> ConditionallyIncludeReconciled(
        this Expression<Func<BankTransaction, bool>> predicate, GetBankTransactionsQuery query) =>
        query.IncludeReconciled ? predicate.IncludeStatus(BankTransactionStatus.Reconciled) : predicate;

    static Expression<Func<BankTransaction, bool>> IncludeStatus(this Expression<Func<BankTransaction, bool>> predicate, BankTransactionStatus status) =>
        predicate.Or(transaction => transaction.Status == status);

    static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2)
    {
        var body = Expression.AndAlso(predicate1.Body, predicate2.Body);
        return Expression.Lambda<Func<T, bool>>(body, predicate1.Parameters[0]);
    }

    static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2)
    {
        var body = Expression.OrElse(predicate1.Body, predicate2.Body);
        return Expression.Lambda<Func<T, bool>>(body, predicate1.Parameters[0]);
    }
}
