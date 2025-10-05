using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.GetPayOuts;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPayOutsShell
{
    static readonly IQuery<PayOut> payOutQuery = Query<PayOut>();

    public static EitherAsync<Failure<Unit>, Seq<PayOutDetails>> GetPayOuts(
        IBankingDateProvider bankingDateProvider, IEntityReader reader, ITimeConverter timeConverter, CancellationToken cancellationToken) =>
        from payOutEntities in reader.QueryWithETag(payOutQuery, cancellationToken).MapReadError()
        let userIds = toHashSet(payOutEntities.Map(entity => entity.Value.UserId))
        from userExcerpts in ReadUserExcerpts(reader, userIds, cancellationToken)
        from importOption in reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption()
        let latestImportTimestamp = importOption.Map(import => import.Timestamp)
        let output = GetPayOutsCore(bankingDateProvider, timeConverter, new(payOutEntities, userExcerpts, latestImportTimestamp))
        select output.PayOuts;
}
