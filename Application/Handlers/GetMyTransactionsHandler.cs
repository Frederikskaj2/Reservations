using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.MyTransactionFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetMyTransactionsHandler
{
    public static EitherAsync<Failure, MyTransactions> Handle(IFormatter formatter, OrderingOptions options, IPersistenceContextFactory contextFactory, UserId userId) =>
        GetMyTransactions(formatter, options, userId, CreateContext(contextFactory));

    static EitherAsync<Failure, MyTransactions> GetMyTransactions(IFormatter formatter, OrderingOptions options, UserId userId, IPersistenceContext context) =>
        from user in ReadUser(context, userId)
        from transactions in ReadTransactionsForUser(context, userId)
        select CreateMyTransactions(formatter, options, user, transactions);

}
