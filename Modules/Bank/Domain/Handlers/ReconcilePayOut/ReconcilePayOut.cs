using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class ReconcilePayOut
{
    public static ReconcilePayOutOutput ReconcilePayOutCore(ReconcilePayOutInput input) =>
        CreateOutput(input, -input.BankTransaction.Amount);

    static ReconcilePayOutOutput CreateOutput(ReconcilePayOutInput input, Amount amount) =>
        CreateOutput(input, CreateTransaction(input, amount), amount);

    static ReconcilePayOutOutput CreateOutput(ReconcilePayOutInput input, Transaction transaction, Amount amount) =>
        new(
            input.BankTransaction with { Status = BankTransactionStatus.Reconciled, ReconciledTransactionId = input.TransactionId },
            AddAudit(input.Command, input.User.AddTransaction(transaction), transaction),
            transaction,
            amount);

    static User AddAudit(ReconcilePayOutCommand command, User user, Transaction transaction) =>
        user with { Audits = user.Audits.Add(UserAudit.PayOut(command.Timestamp, command.AdministratorId, transaction.TransactionId)) };

    static Transaction CreateTransaction(ReconcilePayOutInput input, Amount amount) =>
        new(
            input.TransactionId,
            input.BankTransaction.Date,
            input.Command.AdministratorId,
            input.Command.Timestamp,
            Activity.PayOut,
            input.User.UserId,
            None,
            PayOut(amount, GetExcessAmount(amount, input.User)));

    static Amount GetExcessAmount(Amount amount, User user) =>
        GetExcessAmount(amount, user.Accounts[Account.AccountsPayable]);

    static Amount GetExcessAmount(Amount amount, Amount accountsPayable) =>
        amount > -accountsPayable ? amount + accountsPayable : Amount.Zero;
}
