using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.SendDebtReminders;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class SendDebtRemindersShell
{
    public static EitherAsync<Failure<Unit>, Unit> SendDebtReminders(
        IBankEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendDebtRemindersCommand command,
        CancellationToken cancellationToken) =>
        from debtReminderEntity in reader.ReadWithETag<DebtReminder>(DebtReminder.SingletonId, cancellationToken).NotFoundToOption()
        from importEntity in reader.Read<Import>(Import.SingletonId, cancellationToken).NotFoundToOption()
        from _ in UpdateDebtRemindersWhenThereIsANewBankTransactionsImport(
            emailService, options, reader, writer, command, debtReminderEntity, importEntity, cancellationToken)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> UpdateDebtRemindersWhenThereIsANewBankTransactionsImport(
        IBankEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendDebtRemindersCommand command,
        Option<ETaggedEntity<DebtReminder>> debtReminderEntityOption,
        Option<Import> importOption,
        CancellationToken cancellationToken) =>
        (debtReminderEntityOption.Map(entity => entity.Value).Case, importOption.Case) switch
        {
            (DebtReminder debtReminder, Import import) when debtReminder.LatestExecutionTime < import.Timestamp =>
                SendDebtReminders(emailService, options, reader, writer, command, debtReminderEntityOption, cancellationToken),
            (null, Import) => SendDebtReminders(emailService, options, reader, writer, command, debtReminderEntityOption, cancellationToken),
            _ => unit,
        };

    static EitherAsync<Failure<Unit>, Unit> SendDebtReminders(
        IBankEmailService emailService,
        OrderingOptions options,
        IEntityReader reader,
        IEntityWriter writer,
        SendDebtRemindersCommand command,
        Option<ETaggedEntity<DebtReminder>>  debtReminderEntityOption,
        CancellationToken cancellationToken) =>
        from userEntities in reader.QueryWithETag(GetQuery(command.Timestamp.Minus(options.RemindUsersAboutDebtInterval)), cancellationToken).MapReadError()
        let output = SendDebtReminderCore(new(command, userEntities.ToValues()))
        from _1 in Write(writer, userEntities, debtReminderEntityOption, output.UsersToUpdate, output.DebtReminder, cancellationToken)
        from _2 in SendDebtReminderEmails(emailService, options, output.UsersToRemind, cancellationToken)
        select unit;

    static IQuery<User> GetQuery(Instant previousReminder) =>
        QueryFactory.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.LatestDebtReminder <= previousReminder);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        Seq<ETaggedEntity<User>> userEntities,
        Option<ETaggedEntity<DebtReminder>> debtReminderEntityOption,
        Seq<User> userToUpdate,
        DebtReminder debtReminder,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(userEntities).Add(debtReminderEntityOption.ToSeq()),
                tracker => tracker.Update(userToUpdate).AddOrUpdate(debtReminder),
                cancellationToken)
            .MapWriteError();

    static EitherAsync<Failure<Unit>, Unit> SendDebtReminderEmails(
        IBankEmailService emailService, OrderingOptions options, Seq<User> users, CancellationToken cancellationToken) =>
        from _ in users.Map(user => SendDebtReminderEmail(emailService, options, user, cancellationToken)).TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure<Unit>, Unit> SendDebtReminderEmail(
        IBankEmailService emailService, OrderingOptions options, User user, CancellationToken cancellationToken) =>
        emailService.Send(new DebtReminderEmailModel(user.Email(), user.FullName, GetPaymentInformation(options, user)), cancellationToken);

    static PaymentInformation GetPaymentInformation(OrderingOptions options, User user) =>
        new(FromUserId(user.UserId), -user.Balance(), options.PayInAccountNumber);
}
