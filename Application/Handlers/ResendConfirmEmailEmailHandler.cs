using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class ResendConfirmEmailEmailHandler
{
    public static EitherAsync<Failure, Unit> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, ResendConfirmEmailEmailCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        let context2 = context1.UpdateItem<User>(User.GetId(command.UserId), u => u with { Audits = u.Audits.Add(new(command.Timestamp, u.UserId, UserAuditType.RequestResendConfirmEmail)) })
        from _1 in WriteContext(context2)
        let user = context2.Item<User>()
        let sendConfirmEmailEmailModel = new ConfirmEmailEmailModel(command.Timestamp, command.UserId, user.Email(), user.FullName)
        from _2 in emailService.Send(sendConfirmEmailEmailModel).ToRightAsync<Failure, Unit>()
        select unit;
}
