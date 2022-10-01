using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetCleaningScheduleHandler
{
    public static EitherAsync<Failure, Shared.Core.CleaningSchedule> Handle(IPersistenceContextFactory contextFactory, OrderingOptions options, LocalDate date) =>
        GetCleaningSchedule(contextFactory, options, date);
}
