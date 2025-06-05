using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Cleaning.CleaningFunctions;
using static Frederikskaj2.Reservations.Cleaning.SendCleaningScheduleUpdate;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

public static class SendCleaningScheduleUpdateShell
{
    public static EitherAsync<Failure<Unit>, Unit> SendCleaningScheduleUpdate(
        ICleaningEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        SendCleaningScheduleUpdateCommand command,
        CancellationToken cancellationToken) =>
        from currentEntity in ReadCurrentCleaningScheduleEntity(reader, cancellationToken)
        from publishedEntity in ReadPublishedCleaningScheduleEntity(reader, cancellationToken)
        from subscribedEmailUsers in ReadSubscribedEmailUsers(reader, EmailSubscriptions.CleaningScheduleUpdated, cancellationToken)
        let output = SendCleaningScheduleUpdateCore(new(command, currentEntity.EntityValue, publishedEntity))
        from _1 in Write(writer, publishedEntity, output, cancellationToken)
        from _2 in SendCleaningScheduleEmails(
            emailService, subscribedEmailUsers, output.PublishedScheduleEntity.EntityValue, output.DeltaOption, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        OptionalEntity<CleaningSchedule> publishedEntity,
        SendCleaningScheduleUpdateOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.TryAdd(publishedEntity),
                tracker => tracker.AddOrUpdate(output.PublishedScheduleEntity), cancellationToken)
            .MapWriteError();

    static EitherAsync<Failure<Unit>, Unit> SendCleaningScheduleEmails(
        ICleaningEmailService emailService, Seq<EmailUser> users, CleaningSchedule schedule, Option<CleaningTasksDelta> deltaOption,
        CancellationToken cancellationToken) =>
        deltaOption.Case switch
        {
            CleaningTasksDelta delta => SendCleaningScheduleEmails(emailService, users, schedule, delta, cancellationToken),
            _ => unit,
        };

    static EitherAsync<Failure<Unit>, Unit> SendCleaningScheduleEmails(
        ICleaningEmailService emailService, Seq<EmailUser> users, CleaningSchedule schedule, CleaningTasksDelta delta, CancellationToken cancellationToken) =>
        emailService.Send(new(schedule, delta), users, cancellationToken).ToRightAsync<Failure<Unit>, Unit>();
}
