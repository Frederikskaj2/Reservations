using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;

namespace Frederikskaj2.Reservations.Orders;

public static class GetCreditorsShell
{
    static readonly IProjectedQuery<User> query = QueryFactory.Query<User>()
        .Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.Roles.HasFlag(Roles.Resident) &&
                       user.Accounts[Account.AccountsPayable] < 0)
        .Project();

    public static EitherAsync<Failure<Unit>, Seq<User>> GetCreditors(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Query(query, cancellationToken).MapReadError();
}
