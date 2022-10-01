using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetWeeklyLockBoxCodesHandler
{
    public static EitherAsync<Failure, IEnumerable<WeeklyLockBoxCodes>> Handle(IPersistenceContextFactory contextFactory, LocalDate date) =>
        from context in ReadLockBoxCodesContext(CreateContext(contextFactory), date)
        from _ in WriteContext(context)
        select CreateWeeklyLockBoxCodes(context.Item<LockBoxCodes>());
}
