using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class SendNewPasswordEmailShell
{
    public static EitherAsync<Failure<Unit>, Unit> SendNewPasswordEmail(
        IUsersEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        SendNewPasswordEmailCommand command,
        CancellationToken cancellationToken) =>
        ReadUserWithRequestNewPasswordAudit<Unit>(reader, writer, command.Timestamp, command.Email, cancellationToken)
            .BiBind(
                user => emailService
                    .Send(new NewPasswordEmailModel(command.Timestamp, user.Email(), user.FullName), cancellationToken)
                    .ToRightAsync<Failure<Unit>, Unit>(),
                NotFoundToOk);

    static EitherAsync<Failure<Unit>, Unit> NotFoundToOk(Failure<Unit> failure) =>
        failure.Status is HttpStatusCode.NotFound ? unit : failure;
}
