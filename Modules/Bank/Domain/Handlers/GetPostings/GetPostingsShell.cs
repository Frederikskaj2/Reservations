using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;
using static Frederikskaj2.Reservations.Bank.PostingFunctions;

namespace Frederikskaj2.Reservations.Bank;

public static class GetPostingsShell
{
    public static EitherAsync<Failure<Unit>, PostingsForMonth> GetPostings(IEntityReader reader, GetPostingsQuery query, CancellationToken cancellationToken) =>
        from postings in GetPostingsV1OrV2(reader, query.Month, cancellationToken).MapReadError()
        let userIds = toHashSet(postings.Map(posting => posting.ResidentId))
        from userExcerpts in ReadUserExcerpts(reader, userIds, cancellationToken)
        let users = toHashMap(userExcerpts.Map(userExcerpt => (userExcerpt.UserId, userExcerpt)))
        select new PostingsForMonth(postings, users);
}
