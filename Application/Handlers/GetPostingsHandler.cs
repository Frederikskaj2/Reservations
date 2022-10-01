using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PostingFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetPostingsHandler
{
    public static EitherAsync<Failure, IEnumerable<Posting>> Handle(IPersistenceContextFactory contextFactory, LocalDate month) =>
        GetPostingsForMonth(CreateContext(contextFactory), month);
}
