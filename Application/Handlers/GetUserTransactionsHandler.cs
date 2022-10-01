using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetUserTransactionsHandler
{
    public static EitherAsync<Failure, UserTransactions> Handle(IPersistenceContextFactory contextFactory, IFormatter formatter, UserId userId) =>
        ReadUserTransactions(formatter, CreateContext(contextFactory), userId);
}
