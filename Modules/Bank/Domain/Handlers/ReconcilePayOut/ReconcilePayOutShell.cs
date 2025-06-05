using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;
using static Frederikskaj2.Reservations.Bank.ReconcilePayOut;

namespace Frederikskaj2.Reservations.Bank;

public static class ReconcilePayOutShell
{
    public static EitherAsync<Failure<Unit>, BankTransaction> ReconcilePayOut(
        IBankEmailService emailService,
        IEntityReader reader,
        IEntityWriter writer,
        ReconcilePayOutCommand command,
        CancellationToken cancellationToken) =>
        from payOutEntity in reader.ReadWithETag<PayOut>(command.PayOutId, cancellationToken).MapReadError()
        from bankTransactionEntity in reader.ReadWithETag<BankTransaction>(command.BankTransactionId, cancellationToken).MapReadError()
        from _1 in ValidateAmount(payOutEntity.Value, bankTransactionEntity.Value).ToAsync()
        from userEntity in reader.ReadWithETag<User>(payOutEntity.Value.UserId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        let output = ReconcilePayOutCore(new(command, bankTransactionEntity.Value, userEntity.Value, transactionId))
        from _2 in writer.Write(
            collector => collector.Add(payOutEntity).Add(bankTransactionEntity).Add(userEntity),
            tracker => tracker.Remove(payOutEntity).Update(output.BankTransaction).Update(output.User).Add(output.Transaction),
            cancellationToken).MapWriteError()
        from _3 in SendPayOutEmail(emailService, output.User, output.Amount, cancellationToken)
        select output.BankTransaction;

    static Either<Failure<Unit>, Unit> ValidateAmount(PayOut payOut, BankTransaction transaction) =>
        payOut.Amount == -transaction.Amount
            // This check isn't needed because payOut.Amount is always positive but just in case...
            ? transaction.Amount < Amount.Zero
                ? unit
                : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out amount must be negative.")
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out amount doesn't match transaction amount.");

    static EitherAsync<Failure<Unit>, Unit> SendPayOutEmail(IBankEmailService emailService, User user, Amount amount, CancellationToken cancellationToken) =>
        emailService.Send(new PayOutEmailModel(user.Email(), user.FullName, amount), cancellationToken).ToRightAsync<Failure<Unit>, Unit>();
}
