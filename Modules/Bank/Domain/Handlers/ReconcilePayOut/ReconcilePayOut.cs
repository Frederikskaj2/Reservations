using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Net;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class ReconcilePayOut
{
    public static Either<Failure<Unit>, ReconcilePayOutOutput> ReconcilePayOutCore(ReconcilePayOutInput input) =>
        CreateOutput(input, -input.BankTransaction.Amount);

    static Either<Failure<Unit>, ReconcilePayOutOutput> CreateOutput(ReconcilePayOutInput input, Amount amount) =>
        CreateOutput(input, CreateTransaction(input, amount), amount);

    static Either<Failure<Unit>, ReconcilePayOutOutput> CreateOutput(ReconcilePayOutInput input, Transaction transaction, Amount amount) =>
        from _ in ValidateAmount(input.PayOut, input.BankTransaction)
        from payOut in UpdatePayOut(input.Command, input.PayOut, input.BankTransaction.BankTransactionId, input.TransactionId)
        let bankTransaction = input.BankTransaction with { Status = BankTransactionStatus.Reconciled, ReconciledTransactionId = input.TransactionId }
        let resident = AddResidentAudit(input.Command, input.Resident.AddTransaction(transaction), transaction)
        select new ReconcilePayOutOutput(payOut, bankTransaction, resident, transaction, amount);

    static Either<Failure<Unit>, Unit> ValidateAmount(PayOut payOut, BankTransaction transaction) =>
        payOut.Amount == -transaction.Amount
            // This check isn't needed because payOut.Amount is always positive but just in
            // case...
            ? transaction.Amount < Amount.Zero
                ? unit
                : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out amount must be negative.")
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out amount doesn't match transaction amount.");

    static Either<Failure<Unit>, PayOut> UpdatePayOut(
        ReconcilePayOutCommand command, PayOut payOut, BankTransactionId bankTransactionId, TransactionId transactionId) =>
        payOut.Status is PayOutStatus.InProgress
            ? payOut with
            {
                Status = PayOutStatus.Reconciled,
                Resolution = (PayOutResolution) new Reconciled(command.Timestamp, bankTransactionId, transactionId),
                Audits = payOut.Audits.Add(new(command.Timestamp, command.AdministratorId, PayOutAuditType.Reconcile)),
            }
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Pay-out with status {payOut.Status} cannot be reconciled.");

    static User AddResidentAudit(ReconcilePayOutCommand command, User resident, Transaction transaction) =>
        resident with { Audits = resident.Audits.Add(UserAudit.PayOut(command.Timestamp, command.AdministratorId, transaction.TransactionId)) };

    static Transaction CreateTransaction(ReconcilePayOutInput input, Amount amount) =>
        new(
            input.TransactionId,
            input.BankTransaction.Date,
            input.Command.AdministratorId,
            input.Command.Timestamp,
            Activity.PayOut,
            input.Resident.UserId,
            None,
            PayOut(amount, GetExcessAmount(amount, input.Resident)));

    static Amount GetExcessAmount(Amount amount, User user) =>
        GetExcessAmount(amount, user.Accounts[Account.AccountsPayable]);

    static Amount GetExcessAmount(Amount amount, Amount accountsPayable) =>
        amount > -accountsPayable ? amount + accountsPayable : Amount.Zero;
}
