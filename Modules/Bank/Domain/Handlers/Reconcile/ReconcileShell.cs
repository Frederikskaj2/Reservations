using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.Reconcile;
using static Frederikskaj2.Reservations.Orders.PaymentFunctions;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Persistence.QueryFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class ReconcileShell
{
    public static EitherAsync<Failure<Unit>, BankTransaction> Reconcile(
        IBankEmailService emailService,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        ReconcileCommand command,
        CancellationToken cancellationToken) =>
        from bankTransactionEntity in reader.ReadWithETag<BankTransaction>(command.BankTransactionId, cancellationToken).MapReadError()
        from userEntity in reader.ReadWithETag<User>(command.ResidentId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        from import in reader.Read<Import>(Import.SingletonId, cancellationToken).MapReadError()
        let latestImportDate = timeConverter.GetDate(import.Timestamp)
        from payOutEntities in reader.QueryWithETag(GetUserPayOutsQuery(command.ResidentId), cancellationToken).MapReadError()
        let input = new ReconcileInput(command, bankTransactionEntity.Value, userEntity.Value, transactionId, latestImportDate, payOutEntities)
        let output = ReconcileCore(timeConverter, input)
        from _1 in Write(writer, bankTransactionEntity, userEntity, output, cancellationToken)
        from _2 in SendEmail(emailService, options, output.User, output.BankTransaction.Amount, cancellationToken)
        from _3 in ConfirmOrders(jobScheduler, output.BankTransaction.Amount).ToRightAsync<Failure<Unit>, Unit>()
        select output.BankTransaction;

    static IQuery<PayOut> GetUserPayOutsQuery(UserId userId) =>
        Query<PayOut>().Where(payOut => payOut.UserId == userId);

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<BankTransaction> bankTransactionEntity,
        ETaggedEntity<User> userEntity,
        ReconcileOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(bankTransactionEntity).Add(userEntity),
                tracker => DeletePayOutIfMatched(output.PayOutToDelete, tracker).Update(output.BankTransaction).Update(output.User).Add(output.Transaction),
                cancellationToken)
            .MapWriteError();

    static EntityTracker DeletePayOutIfMatched(Option<ETaggedEntity<PayOut>> entityOption, EntityTracker tracker) =>
        entityOption.Case switch
        {
            ETaggedEntity<PayOut> entity => tracker.Remove<PayOut>(entity.Value.PayOutId, entity.ETag),
            _ => tracker,
        };

    static EitherAsync<Failure<Unit>, Unit> SendEmail(
        IBankEmailService emailService, OrderingOptions options, User user, Amount amount, CancellationToken cancellationToken) =>
        amount > Amount.Zero
            ? SendPayInReceivedEmail(emailService, options, user, amount, cancellationToken)
            : SendPayOutEmail(emailService, user, amount, cancellationToken);

    static EitherAsync<Failure<Unit>, Unit> SendPayInReceivedEmail(
        IBankEmailService emailService, OrderingOptions options, User user, Amount amount, CancellationToken cancellationToken) =>
        emailService.Send(new PayInEmailModel(user.Email(), user.FullName, amount, GetPaymentInformation(options, user)), cancellationToken)
            .ToRightAsync<Failure<Unit>, Unit>();

    static EitherAsync<Failure<Unit>, Unit> SendPayOutEmail(IBankEmailService emailService, User user, Amount amount, CancellationToken cancellationToken) =>
        emailService.Send(new PayOutEmailModel(user.Email(), user.FullName, amount), cancellationToken).ToRightAsync<Failure<Unit>, Unit>();

    static Unit ConfirmOrders(IJobScheduler jobScheduler, Amount amount) =>
        amount > Amount.Zero ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;
}
