using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesFunctions;

namespace Frederikskaj2.Reservations.LockBox;

public static class SendLockBoxCodesOverviewShell
{
    public static EitherAsync<Failure<Unit>, EmailAddress> SendLockBoxCodesOverview(
        ILockBoxEmailService emailService,
        IEntityReader reader,
        SendLockBoxCodesOverviewCommand command,
        CancellationToken cancellationToken) =>
        from user in reader.Read<User>(command.UserId, cancellationToken).MapReadError()
        from lockBoxCodes in ReadLockBoxCodes(reader, cancellationToken)
        let weeklyLockBoxCodes = CreateWeeklyLockBoxCodes(lockBoxCodes)
        from _ in SendWeeklyLockBoxCodes(emailService, user, weeklyLockBoxCodes, cancellationToken)
        select user.Email();

    static EitherAsync<Failure<Unit>, Unit> SendWeeklyLockBoxCodes(
        ILockBoxEmailService emailService, User user, Seq<WeeklyLockBoxCodes> lockBoxCodes, CancellationToken cancellationToken) =>
        emailService.Send(new(user.Email(), user.FullName, lockBoxCodes), cancellationToken);
}
