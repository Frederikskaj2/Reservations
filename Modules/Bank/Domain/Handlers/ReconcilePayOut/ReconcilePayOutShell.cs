using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
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
        from residentEntity in reader.ReadWithETag<User>(payOutEntity.Value.ResidentId, cancellationToken).MapReadError()
        from inProgressPayOutEntity in reader.ReadWithETag<InProgressPayOut>(residentEntity.Value.UserId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        from output in ReconcilePayOutCore(new(command, payOutEntity.Value, bankTransactionEntity.Value, residentEntity.Value, transactionId)).ToAsync()
        from _1 in writer.Write(
            collector => collector.Add(payOutEntity).Add(bankTransactionEntity).Add(residentEntity).Add(inProgressPayOutEntity),
            tracker => tracker
                .Update(output.PayOut)
                .Update(output.BankTransaction)
                .Update(output.Resident)
                .Add(output.Transaction)
                .Remove(inProgressPayOutEntity),
            cancellationToken).MapWriteError()
        from _2 in SendPayOutEmail(emailService, output.Resident, output.Amount, cancellationToken)
        select output.BankTransaction;

    static EitherAsync<Failure<Unit>, Unit> SendPayOutEmail(IBankEmailService emailService, User user, Amount amount, CancellationToken cancellationToken) =>
        emailService.Send(new PayOutEmailModel(user.Email(), user.FullName, amount), cancellationToken).ToRightAsync<Failure<Unit>, Unit>();
}
