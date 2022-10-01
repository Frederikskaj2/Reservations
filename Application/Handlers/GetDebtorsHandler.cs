using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.PayInFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetDebtorsHandler
{
    public static EitherAsync<Failure, IEnumerable<Debtor>> Handle(IPersistenceContextFactory contextFactory) =>
        GetDebtors(DatabaseFunctions.CreateContext(contextFactory));
}
