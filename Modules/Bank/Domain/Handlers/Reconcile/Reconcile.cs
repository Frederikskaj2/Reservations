using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class Reconcile
{
    public static ReconcileOutput ReconcileCore(ReconcileInput input) =>
        CreateOutput(input, CreateTransaction(input));

    static Transaction CreateTransaction(ReconcileInput input) =>
        input.BankTransaction.Amount > Amount.Zero
            ? CreatePayInTransaction(input)
            : CreatePayOutTransaction(input);

    static Transaction CreatePayInTransaction(ReconcileInput input) =>
        new(
            input.TransactionId,
            input.BankTransaction.Date,
            input.Command.AdministratorId,
            input.Command.Timestamp,
            Activity.PayIn,
            input.Resident.UserId,
            None,
            PayIn(input.BankTransaction.Amount, GetPayInExcessAmount(input.Resident, input.BankTransaction.Amount)));

    static Amount GetPayInExcessAmount(User resident, Amount amount) =>
        GetPayInExcessAmount(amount, resident.Accounts[Account.AccountsReceivable]);

    static Amount GetPayInExcessAmount(Amount amount, Amount accountsReceivable) =>
        amount > accountsReceivable ? amount - accountsReceivable : Amount.Zero;

    static Transaction CreatePayOutTransaction(ReconcileInput input) =>
        new(
            input.TransactionId,
            input.BankTransaction.Date,
            input.Command.AdministratorId,
            input.Command.Timestamp,
            Activity.PayOut,
            input.Resident.UserId,
            None,
            PayOut(-input.BankTransaction.Amount, GetPayOutExcessAmount(-input.BankTransaction.Amount, input.Resident)));

    static Amount GetPayOutExcessAmount(Amount amount, User resident) =>
        GetPayOutExcessAmount(amount, resident.Accounts[Account.AccountsPayable]);

    static Amount GetPayOutExcessAmount(Amount amount, Amount accountsPayable) =>
        amount > -accountsPayable ? amount + accountsPayable : Amount.Zero;

    static ReconcileOutput CreateOutput(ReconcileInput input, Transaction transaction) =>
        new(
            input.BankTransaction with { Status = BankTransactionStatus.Reconciled, ReconciledTransactionId = input.TransactionId },
            UpdateResident(input, transaction),
            transaction,
            TryUpdateResidentPayOut(input));

    static User UpdateResident(ReconcileInput input, Transaction transaction) =>
        AddResidentAudit(input.Command, input.BankTransaction.Amount, input.Resident.AddTransaction(transaction), transaction.TransactionId);

    static User AddResidentAudit(ReconcileCommand command, Amount amount, User resident, TransactionId transactionId) =>
        resident with { Audits = resident.Audits.Add(CreateAudit(command, amount, transactionId)) };

    static UserAudit CreateAudit(ReconcileCommand command, Amount amount, TransactionId transactionId) =>
        amount > Amount.Zero
            ? UserAudit.PayIn(command.Timestamp, command.AdministratorId, transactionId)
            : UserAudit.PayOut(command.Timestamp, command.AdministratorId, transactionId);

    static Option<PayOutToReconcile> TryUpdateResidentPayOut(ReconcileInput input) =>
        input.ResidentPayOutPairOption.Case switch
        {
            PayOutPair payOutPair when payOutPair.PayOutEntity.Value.Amount == -input.BankTransaction.Amount =>
                new PayOutToReconcile(
                    payOutPair.PayOutEntity,
                    payOutPair.InProgressPayOutEntity,
                    UpdatePayOut(input.Command, payOutPair.PayOutEntity.Value, input.BankTransaction.BankTransactionId, input.TransactionId)),
            _ => None,
        };

    static PayOut UpdatePayOut(ReconcileCommand command, PayOut payOut, BankTransactionId bankTransactionId, TransactionId transactionId) =>
        payOut with
        {
            Status = PayOutStatus.Reconciled,
            Resolution = (PayOutResolution) new Reconciled(command.Timestamp, bankTransactionId, transactionId),
            Audits = payOut.Audits.Add(new(command.Timestamp, command.AdministratorId, PayOutAuditType.Reconcile)),
        };
}
