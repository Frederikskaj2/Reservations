using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Orders;

public static class GetResidentsShell
{
    static readonly IProjectedQuery<User> query = QueryFactory.Query<User>()
        .Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.Roles.HasFlag(Roles.Resident))
        .Project();

    public static EitherAsync<Failure<Unit>, Seq<User>> GetResidents(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Query(query, cancellationToken).MapReadError();
}
