using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UpdateMyUser;

namespace Frederikskaj2.Reservations.Users;

public static class UpdateMyUserShell
{
    public static EitherAsync<Failure<Unit>, User> UpdateMyUser(
        IEntityReader reader, IEntityWriter writer, UpdateMyUserCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from output in UpdateMyUserCore(new(command, userEntity.Value)).ToAsync()
        from _ in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        select output.User;
}
