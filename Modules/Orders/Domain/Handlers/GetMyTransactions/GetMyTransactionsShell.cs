using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.GetMyTransactions;

namespace Frederikskaj2.Reservations.Orders;

public static class GetMyTransactionsShell
{
    public static EitherAsync<Failure<Unit>, MyTransactions> GetMyTransactions(
        OrderingOptions options, IEntityReader reader, GetMyTransactionsQuery query, CancellationToken cancellationToken) =>
        from user in reader.Read<User>(query.ResidentId, cancellationToken).MapReadError()
        from transactions in reader.Query(GetQuery(query.ResidentId), cancellationToken).MapReadError()
        let output = GetMyTransactionsCore(options, new(user, transactions))
        select output.MyTransactions;

    static IProjectedQuery<Transaction> GetQuery(UserId residentId) =>
        QueryFactory.Query<Transaction>()
            .Where(transaction => transaction.ResidentId == residentId)
            .OrderBy(transaction => transaction.Date)
            .OrderBy(transaction => transaction.TransactionId)
            .Project();
}
