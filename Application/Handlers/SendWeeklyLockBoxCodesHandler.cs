using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SendWeeklyLockBoxCodesHandler
{
    public static EitherAsync<Failure, EmailAddress> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, SendWeeklyLockBoxCodesCommand command) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        from context2 in ReadLockBoxCodesContext(context1, command.Date)
        from _1 in WriteContext(context2)
        let user = context2.Item<User>()
        let lockBoxCodes = CreateWeeklyLockBoxCodes(context2.Item<LockBoxCodes>())
        from _ in SendLockBoxCodesEmail(emailService, user, lockBoxCodes)
        select user.Email();
}
