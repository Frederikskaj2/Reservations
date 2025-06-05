using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Orders;

public static class GetResidentTransactionsShell
{
    public static EitherAsync<Failure<Unit>, ResidentTransactions> GetResidentTransactions(
        IEntityReader reader, GetResidentTransactionsQuery query, CancellationToken cancellationToken) =>
        from userExcerptOption in ReadUserExcerpt(reader, query.UserId, cancellationToken)
        from userExcerpt in GetUserExcerpt(query.UserId, userExcerptOption).ToAsync()
        from transactions in reader.Query(GetQuery(query.UserId), cancellationToken).MapReadError()
        select new ResidentTransactions(userExcerpt, transactions);

    static Either<Failure<Unit>, UserExcerpt> GetUserExcerpt(UserId userId, Option<UserExcerpt> userExcerptOption) =>
        userExcerptOption.Case switch
        {
            UserExcerpt userExcerpt => userExcerpt,
            _ => Failure.New(HttpStatusCode.NotFound, $"User with ID {userId} does not exist."),
        };

    static IProjectedQuery<Transaction> GetQuery(UserId userId) =>
        QueryFactory.Query<Transaction>()
            .Where(transaction => transaction.ResidentId == userId)
            .OrderBy(transaction => transaction.Date)
            .OrderBy(transaction => transaction.TransactionId)
            .Project();
}
