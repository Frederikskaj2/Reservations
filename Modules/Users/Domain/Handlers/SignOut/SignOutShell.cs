using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.SignOut;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class SignOutShell
{
    public static EitherAsync<Failure<Unit>, Unit> SignOut(
        IEntityReader reader, IEntityWriter writer, SignOutCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).NotFoundToForbidden().MapReadError()
        let output = SignOutCore(new(command, userEntity.Value))
        from _ in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        select unit;
}
