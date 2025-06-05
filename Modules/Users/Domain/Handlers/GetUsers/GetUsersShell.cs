using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Users;

public static class GetUsersShell
{
    static readonly IQuery<User> usersQuery = QueryFactory.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted)).Project();

    public static EitherAsync<Failure<Unit>, Seq<User>> GetUsers(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Query(usersQuery, cancellationToken).MapReadError();
}
