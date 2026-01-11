using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Orders;

public static class GetResidentTransactionsShell
{
    public static EitherAsync<Failure<Unit>, ResidentTransactions> GetResidentTransactions(
        IEntityReader reader, GetResidentTransactionsQuery query, CancellationToken cancellationToken) =>
        from userExcerpt in ReadUserExcerpt(reader, query.UserId, cancellationToken)
        from transactions in reader.Query(GetQuery(query.UserId), cancellationToken).MapReadError()
        select new ResidentTransactions(userExcerpt, transactions);

    static IProjectedQuery<Transaction> GetQuery(UserId userId) =>
        Query<Transaction>()
            .Where(transaction => transaction.ResidentId == userId)
            .OrderBy(transaction => transaction.Date)
            .OrderBy(transaction => transaction.TransactionId)
            .Project();
}
