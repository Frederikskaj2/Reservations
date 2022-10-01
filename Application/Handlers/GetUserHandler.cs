using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class GetUserHandler
{
    public static EitherAsync<Failure, UserDetails> Handle(IPersistenceContextFactory contextFactory, UserId userId) =>
        from user in ReadUser(CreateContext(contextFactory), userId)
        let userIds = toHashSet(user.Audits.Where(audit => audit.UserId.HasValue).Map(audit => audit.UserId!.Value))
        from userFullNames in ReadUserFullNames(CreateContext(contextFactory), userIds)
        let hashMap = toHashMap(userFullNames.Map(u => (u.UserId, u.FullName)))
        select CreateUserDetails(user, hashMap);
}
