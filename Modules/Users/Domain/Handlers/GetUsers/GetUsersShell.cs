using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Users;

public static class GetUsersShell
{
    static readonly IQuery<User> usersQuery = Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted)).Project();

    public static EitherAsync<Failure<Unit>, Seq<User>> GetUsers(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Query(usersQuery, cancellationToken).MapReadError();
}
