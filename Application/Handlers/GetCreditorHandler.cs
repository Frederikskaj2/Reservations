using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

public static class GetCreditorHandler
{

    public static EitherAsync<Failure, Creditor> Handle(IPersistenceContextFactory contextFactory, UserId userId) =>
        PayOutFunctions.GetCreditor(DatabaseFunctions.CreateContext(contextFactory), userId);
}
