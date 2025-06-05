using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPayOutsShell
{
    static readonly IQuery<PayOut> payOutQuery = QueryFactory.Query<PayOut>();

    public static EitherAsync<Failure<Unit>, Seq<PayOutDetails>> GetPayOuts(IEntityReader reader, CancellationToken cancellationToken) =>
        from payOutEntities in reader.QueryWithETag(payOutQuery, cancellationToken).MapReadError()
        let userIds = toHashSet(payOutEntities.Map(entity => entity.Value.UserId))
        from userExcerpts in ReadUserExcerpts(reader, userIds, cancellationToken)
        let usersHashMap = toHashMap(userExcerpts.Map(userExcerpt => (userExcerpt.UserId, userExcerpt)))
        select payOutEntities.Map(entity => new PayOutDetails(entity.Value, entity.ETag, usersHashMap[entity.Value.UserId]));
}
