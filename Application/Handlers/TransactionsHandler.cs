using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Net;
using static Frederikskaj2.Reservations.Application.UserBalanceFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class TransactionsHandler
{
    public static EitherAsync<Failure, Unit> Handle(DeleteTransactionCommand command, IPersistenceContextFactory contextFactory) =>
        DeleteTransaction(command, DatabaseFunctions.CreateContext(contextFactory));

    static EitherAsync<Failure, Unit> DeleteTransaction(DeleteTransactionCommand command, IPersistenceContext context) =>
        from context1 in DatabaseFunctions.ReadTransactionAndUserContext(context, command.TransactionId)
        from _1 in ValidateTransactionDelete(context1.Item<Transaction>())
        let transaction = context1.Item<Transaction>()
        let context2 = context1.UpdateItem<User>(user => DeleteTransactionFromUser(user, transaction))
        let context3 = context2.DeleteItem<Transaction>()
        from _2 in DatabaseFunctions.WriteContext(context3)
        select unit;

    static EitherAsync<Failure, Unit> ValidateTransactionDelete(Transaction transaction) =>
        transaction.Activity is Activity.PayIn or Activity.PayOut
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Cannot delete transaction this is not payin or payout.");

    static User DeleteTransactionFromUser(User user, Transaction transaction) =>
        user with
        {
            Accounts = UpdateAccounts(user.Accounts, transaction),
            Audits = DeleteAudit(user.Audits, transaction.TransactionId)
        };

    static AccountAmounts UpdateAccounts(AccountAmounts accounts, Transaction transaction) =>
        TryEqualizeAccountsReceivableAndAccountsPayable(accounts.ApplyReverse(transaction.Amounts));

    static Seq<UserAudit> DeleteAudit(Seq<UserAudit> audits, TransactionId transactionId) =>
        audits.Filter(audit => audit.TransactionId != transactionId);
}
