using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.DeleteMyUser;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class DeleteMyUserShell
{
    public static EitherAsync<Failure<Unit>, DeleteUserStatus> DeleteMyUser(
        IJobScheduler jobScheduler, IEntityReader reader, IEntityWriter writer, DeleteMyUserCommand command, CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).NotFoundToForbidden().MapReadError()
        from output in DeleteMyUserCore(new(command, userEntity.Value)).ToAsync()
        from _1 in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        from _2 in SendDeleteUserEmail(jobScheduler, output.Status).ToRightAsync<Failure<Unit>, Unit>()
        select output.Status;

    static Unit SendDeleteUserEmail(IJobScheduler jobScheduler, DeleteUserStatus status) =>
        status is DeleteUserStatus.Confirmed ? jobScheduler.Queue(JobName.DeleteUsers) : unit;
}
