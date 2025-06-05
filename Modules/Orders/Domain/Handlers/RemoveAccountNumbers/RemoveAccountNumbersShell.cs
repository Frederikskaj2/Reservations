using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.RemoveAccountNumbers;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class RemoveAccountNumbersShell
{
    static readonly IQuery<User> query = QueryFactory.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.AccountNumber.IsSome);

    public static EitherAsync<Failure<Unit>, Unit> RemoveAccountNumbers(
        OrderingOptions  options, IEntityReader reader, IEntityWriter writer, RemoveAccountNumbersCommand command, CancellationToken cancellationToken) =>
        from userEntities in reader.QueryWithETag(query, cancellationToken).MapReadError()
        let output = RemoveAccountNumbersCore(options, new(command, userEntities.ToValues()))
        from _ in writer.Write(collector => collector.Add(userEntities), tracker => tracker.Update(output.UpdatedUsers), cancellationToken).MapWriteError()
        select unit;
}
