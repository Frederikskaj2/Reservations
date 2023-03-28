using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.YearlySummaryFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetYearlySummaryRangeHandler
{
    public static EitherAsync<Failure, YearlySummaryRange> Handle(IPersistenceContextFactory contextFactory, IDateProvider dateProvider) =>
        GetYearlySummaryRangeOrThisYear(DatabaseFunctions.CreateContext(contextFactory), dateProvider);
}
