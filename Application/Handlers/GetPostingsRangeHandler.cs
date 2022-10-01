using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PostingFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetPostingsRangeHandler
{
    public static EitherAsync<Failure, PostingsRange> Handle(IPersistenceContextFactory contextFactory, IDateProvider dateProvider) =>
        GetPostingsRangeOrThisMonth(CreateContext(contextFactory), dateProvider);
}
