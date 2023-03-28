using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

public static class GetYearlySummaryHandler
{
    public static EitherAsync<Failure, YearlySummary> Handle(IPersistenceContextFactory contextFactory, int year) =>
        YearlySummaryFunctions.GetYearlySummary(DatabaseFunctions.CreateContext(contextFactory), year);
}
