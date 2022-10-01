using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetUsersHandler
{
    public static EitherAsync<Failure, IEnumerable<Shared.Core.User>> Handle(IPersistenceContextFactory contextFactory) =>
        from users in ReadUsers(CreateContext(contextFactory))
        select CreateUsers(users);
}
