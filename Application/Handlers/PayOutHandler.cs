using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.CreditorFactory;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.PayOutFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class PayOutHandler
{
    public static EitherAsync<Failure, Creditor> Handle(IPersistenceContextFactory contextFactory, IEmailService emailService, PayOutCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        from context2 in PayOut(command, context1)
        let user = context2.Item<User>()
        from context3 in TryDeleteUser(emailService, context2, command.Timestamp, command.AdministratorUserId)
        from _1 in DatabaseFunctions.WriteContext(context3)
        from _2 in SendPayOutEmail(emailService, user, command.Amount)
        select CreateCreditor(user);
}
