using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.CleaningFunctions;

namespace Frederikskaj2.Reservations.Cleaning;

public static class SendCleaningScheduleShell
{
    public static EitherAsync<Failure<Unit>, EmailAddress> SendCleaningSchedule(
        ICleaningEmailService emailService, IEntityReader reader, SendCleaningScheduleCommand command, CancellationToken cancellationToken) =>
        from user in reader.Read<User>(command.UserId, cancellationToken).MapReadError()
        from entity in ReadCurrentCleaningScheduleEntity(reader, cancellationToken)
        let schedule = entity.EntityValue
        from _ in SendCleaningScheduleEmail(emailService, user, schedule, cancellationToken)
        select user.Email();

    static EitherAsync<Failure<Unit>, Unit> SendCleaningScheduleEmail(
        ICleaningEmailService emailService, User user, CleaningSchedule schedule, CancellationToken cancellationToken) =>
        emailService
            .Send(
                new(schedule, CleaningTasksDelta.Empty),
                new EmailUser(user.UserId, user.Email(), user.FullName).Cons(),
                cancellationToken)
            .ToRightAsync<Failure<Unit>, Unit>();
}
