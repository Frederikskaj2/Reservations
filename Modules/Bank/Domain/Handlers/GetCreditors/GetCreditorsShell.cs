using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;

namespace Frederikskaj2.Reservations.Bank;

public static class GetCreditorsShell
{
    static readonly IProjectedQuery<User> creditorsQuery = Query<User>()
        .Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.Roles.HasFlag(Roles.Resident) &&
                       user.Accounts[Account.AccountsPayable] < 0)
        .Project();

    static readonly IProjectedQuery<PayOut> inProgressPayOutsQuery = Query<PayOut>()
        .Where(user => user.Status == PayOutStatus.InProgress)
        .Project();

    public static EitherAsync<Failure<Unit>, Seq<User>> GetCreditors(IEntityReader reader, CancellationToken cancellationToken) =>
        from creditors in reader.Query(creditorsQuery, cancellationToken).MapReadError()
        from inProgressPayOuts in reader.Query(inProgressPayOutsQuery, cancellationToken).MapReadError()
        let output = Bank.GetCreditors.GetCreditorsCore(new(creditors, inProgressPayOuts))
        select output.Creditors;
}
