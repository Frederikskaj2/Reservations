using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UpdatePasswordFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SendNewPasswordEmail
{
    public static EitherAsync<Failure, Unit> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, SendNewPasswordEmailCommand command) =>
        GetUserWithRequestNewPasswordAudit(CreateContext(contextFactory), command.Timestamp, command.Email)
            .BiBind(
                user => emailService.Send(new NewPasswordEmailModel(command.Timestamp, user.Email(), user.FullName)).ToRightAsync<Failure, Unit>(),
                IgnoreNotFound);
}
