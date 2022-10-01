using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PayOutFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetCreditorsHandler
{
    public static EitherAsync<Failure, IEnumerable<Creditor>> Handle(IPersistenceContextFactory contextFactory) =>
        GetCreditors(CreateContext(contextFactory));
}
