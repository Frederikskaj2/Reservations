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
        from residentEntity in reader.ReadWithETag<User>(command.ResidentId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        from import in reader.Read<Import>(Import.SingletonId, cancellationToken).MapReadError()
        let latestImportDate = timeConverter.GetDate(import.Timestamp)
        from residentPayOutOption in TryGetResidentPayOut(reader, command.ResidentId, cancellationToken)
        let input = new ReconcileInput(command, bankTransactionEntity.Value, residentEntity.Value, transactionId, residentPayOutOption)
        let output = ReconcileCore(input)
        from _1 in Write(writer, bankTransactionEntity, residentEntity, output, cancellationToken)
        from _2 in SendEmail(emailService, options, output.Resident, output.BankTransaction.Amount, cancellationToken)
        from _3 in ConfirmOrders(jobScheduler, output.BankTransaction.Amount).ToRightAsync<Failure<Unit>, Unit>()
        select output.BankTransaction;

    static EitherAsync<Failure<Unit>, Option<PayOutPair>> TryGetResidentPayOut(
        IEntityReader reader, UserId residentId, CancellationToken cancellationToken) =>
        from payOutEntities in reader.QueryWithETag(GetResidentPayOutsQuery(residentId), cancellationToken).MapReadError()
        from residentPayOutOption in TryGetResidentPayOut(reader, payOutEntities.HeadOrNone(), cancellationToken)
        select residentPayOutOption;

    static EitherAsync<Failure<Unit>, Option<PayOutPair>> TryGetResidentPayOut(
        IEntityReader reader, Option<ETaggedEntity<PayOut>> payOutEntityOption, CancellationToken cancellationToken) =>
        payOutEntityOption.Case switch
        {
            ETaggedEntity<PayOut> entity => GetResidentPayOut(reader, entity, cancellationToken).Map(Some),
            _ => Option<PayOutPair>.None,
        };

    static EitherAsync<Failure<Unit>, PayOutPair> GetResidentPayOut(
        IEntityReader reader, ETaggedEntity<PayOut> payOutEntity, CancellationToken cancellationToken) =>
        from inProgressPayOut in reader.ReadWithETag<InProgressPayOut>(payOutEntity.Value.ResidentId, cancellationToken).MapReadError()
        select new PayOutPair(payOutEntity, inProgressPayOut);

    static IQuery<PayOut> GetResidentPayOutsQuery(UserId residentId) =>
        Query<PayOut>().Where(payOut => payOut.ResidentId == residentId && (payOut.Status == PayOutStatus.InProgress));

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<BankTransaction> bankTransactionEntity,
        ETaggedEntity<User> userEntity,
        ReconcileOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(bankTransactionEntity).Add(userEntity).TryAddResidentPayOut(output.PayOutToReconcile),
                tracker => tracker
                    .Update(output.BankTransaction)
                    .Update(output.Resident)
                    .Add(output.Transaction)
                    .TryRemoveInProgressPayOut(output.PayOutToReconcile)
                    .TryUpdate(output.PayOutToReconcile.Map(payOutToReconcile => payOutToReconcile.UpdatedPayOut)),
                cancellationToken)
            .MapWriteError();

    static EntityCollector TryAddResidentPayOut(this EntityCollector collector, Option<PayOutToReconcile> payOutToReconcileOption) =>
        payOutToReconcileOption.Case switch
        {
            PayOutToReconcile payOutToReconcile => collector.Add(payOutToReconcile.OriginalPayOutEntity).Add(payOutToReconcile.InProgressPayOutEntity),
            _ => collector,
        };

    static EntityTracker TryRemoveInProgressPayOut(this EntityTracker tracker, Option<PayOutToReconcile> option) =>
        option.Case switch
        {
            PayOutToReconcile payOutToReconcile => tracker.Remove(payOutToReconcile.InProgressPayOutEntity),
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
