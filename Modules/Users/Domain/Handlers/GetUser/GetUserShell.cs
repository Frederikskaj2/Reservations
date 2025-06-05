using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Users;

public static class GetUserShell
{
    public static EitherAsync<Failure<Unit>, GetUserResult> GetUser(IEntityReader reader, GetUserQuery query, CancellationToken cancellationToken) =>
        from user in reader.Read<User>(query.UserId, cancellationToken).MapReadError()
        from userFullNames in ReadUserFullNamesMapForUser(reader, user, cancellationToken)
        select new GetUserResult(user, userFullNames);
}
