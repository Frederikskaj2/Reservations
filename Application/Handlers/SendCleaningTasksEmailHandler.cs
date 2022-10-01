using Frederikskaj2.Reservations.Shared.Core;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SendCleaningTasksEmailHandler
{
    public static LanguageExt.EitherAsync<Failure, EmailAddress> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, OrderingOptions options, SendCleaningTasksEmailCommand command) =>
        from user in ReadUser(CreateContext(contextFactory), command.UserId)
        from cleaningSchedule in GetCleaningSchedule(contextFactory, options, command.Date)
        from _ in SendCleaningScheduleEmail(emailService, user, cleaningSchedule)
        select user.Email();
}
