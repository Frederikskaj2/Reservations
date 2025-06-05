using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Users;

public static class GetMyUserShell
{
    public static EitherAsync<Failure<Unit>, User> GetMyUser(IEntityReader reader, GetMyUserQuery query, CancellationToken cancellationToken) =>
        reader.Read<User>(query.UserId, cancellationToken).NotFoundToForbidden().MapReadError();
}
