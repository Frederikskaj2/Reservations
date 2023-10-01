using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;

namespace Frederikskaj2.Reservations.Application;

static class ReimburseFunctions
{
    public static EitherAsync<Failure, IPersistenceContext> Reimburse(ReimburseCommand command, IPersistenceContext context) =>
        from transactionId in CreateTransactionId(context.Factory)
        let transaction = CreateReimburseTransaction(command, transactionId)
        select UpdateUser(command, transactionId, AddUserTransaction(context, transaction));

    static IPersistenceContext UpdateUser(ReimburseCommand command, TransactionId transactionId, IPersistenceContext context) =>
        context.UpdateItem<User>(user => AddAudit(command, transactionId, user));

    static User AddAudit(ReimburseCommand command, TransactionId transactionId, User user) =>
        user with { Audits = user.Audits.Add(CreateAudit(command, transactionId)) };

    static UserAudit CreateAudit(ReimburseCommand command, TransactionId transactionId) =>
        UserAudit.Create(command.Timestamp, command.AdministratorUserId, UserAuditType.Reimburse, transactionId);
}
