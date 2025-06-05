using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class ResendConfirmEmailEmailShell
{
    public static EitherAsync<Failure<Unit>, Unit> ResendConfirmEmailEmail(
        IUsersEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        ResendConfirmEmailEmailCommand command,
        CancellationToken cancellationToken) =>
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        let output = Users.ResendConfirmEmailEmail.ResendConfirmEmailEmailCore(new(command, userEntity.Value))
        from _1 in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken).MapWriteError()
        from _2 in SendEmail(emailService, command, output.User, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendEmail(
        IUsersEmailService emailService, ResendConfirmEmailEmailCommand command, User user, CancellationToken cancellationToken) =>
        emailService.Send(CreateEmail(command, user), cancellationToken).ToRightAsync<Failure<Unit>, Unit>();

    static ConfirmEmailEmailModel CreateEmail(ResendConfirmEmailEmailCommand command, User user) =>
        new(command.Timestamp, command.UserId, user.Email(), user.FullName);
}
