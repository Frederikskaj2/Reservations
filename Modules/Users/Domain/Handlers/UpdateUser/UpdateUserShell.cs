using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UpdateUser;
using static Frederikskaj2.Reservations.Users.UsersFunctions;

namespace Frederikskaj2.Reservations.Users;

public static class UpdateUserShell
{
    public static EitherAsync<Failure<Unit>, UpdateUserResult> UpdateUser(
        IJobScheduler jobScheduler, IEntityReader reader, IEntityWriter writer, UpdateUserCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from output in UpdateUserCore(new(command, userEntity.Value)).ToAsync()
        from _1 in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        from userFullNamesMap in ReadUserFullNamesMapForUser(reader, output.User, cancellationToken)
        from _2 in jobScheduler.Queue(JobName.DeleteUsers).ToRightAsync<Failure<Unit>, Unit>()
        select new UpdateUserResult(output.User, userFullNamesMap);
}
