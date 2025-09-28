using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
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
            input.User.UserId,
            None,
            PayIn(input.BankTransaction.Amount, GetPayInExcessAmount(input.User, input.BankTransaction.Amount)));

    static Amount GetPayInExcessAmount(User user, Amount amount) =>
        GetPayInExcessAmount(amount, user.Accounts[Account.AccountsReceivable]);

    static Amount GetPayInExcessAmount(Amount amount, Amount accountsReceivable) =>
        amount > accountsReceivable ? amount - accountsReceivable : Amount.Zero;

    static Transaction CreatePayOutTransaction(ReconcileInput input) =>
        new(
            input.TransactionId,
            input.BankTransaction.Date,
            input.Command.AdministratorId,
            input.Command.Timestamp,
            Activity.PayOut,
            input.User.UserId,
            None,
            PayOut(-input.BankTransaction.Amount, GetPayOutExcessAmount(-input.BankTransaction.Amount, input.User)));

    static Amount GetPayOutExcessAmount(Amount amount, User user) =>
        GetPayOutExcessAmount(amount, user.Accounts[Account.AccountsPayable]);

    static Amount GetPayOutExcessAmount(Amount amount, Amount accountsPayable) =>
        amount > -accountsPayable ? amount + accountsPayable : Amount.Zero;

    static ReconcileOutput CreateOutput(ReconcileInput input, Transaction transaction) =>
        new(
            input.BankTransaction with { Status = BankTransactionStatus.Reconciled, ReconciledTransactionId = input.TransactionId },
            UpdateUser(input, transaction),
            transaction);

    static User UpdateUser(ReconcileInput input, Transaction transaction) =>
        AddAudit(input.Command, input.BankTransaction.Amount, input.User.AddTransaction(transaction), transaction.TransactionId);

    static User AddAudit(ReconcileCommand command, Amount amount, User user, TransactionId transactionId) =>
        user with { Audits = user.Audits.Add(CreateAudit(command, amount, transactionId)) };

    static UserAudit CreateAudit(ReconcileCommand command, Amount amount, TransactionId transactionId) =>
        amount > Amount.Zero
            ? UserAudit.PayIn(command.Timestamp, command.AdministratorId, transactionId)
            : UserAudit.PayOut(command.Timestamp, command.AdministratorId, transactionId);
}
