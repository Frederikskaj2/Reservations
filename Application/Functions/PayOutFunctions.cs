using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CreditorFactory;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

static class PayOutFunctions
{
    public static EitherAsync<Failure, IEnumerable<Creditor>> GetCreditors(IPersistenceContext context) =>
        from users in MapReadError(context.Untracked.ReadItems(
            context.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.Roles.HasFlag(Roles.Resident) &&
                                                user.Accounts[Account.AccountsPayable] < Amount.Zero)))
        select CreateCreditors(users);

    public static EitherAsync<Failure, IPersistenceContext> PayOut(PayOutCommand command, IPersistenceContext context) =>
        from transactionId in CreateTransactionId(context.Factory)
        let transaction = CreatePayOutTransaction(command, transactionId, GetExcessAmount(context, command.Amount))
        select UpdateUser(command, transactionId, AddUserTransaction(context, transaction));

    static Amount GetExcessAmount(IPersistenceContext context, Amount amount) =>
        GetExcessAmount(amount, context.Item<User>().Accounts[Account.AccountsPayable]);

    static Amount GetExcessAmount(Amount amount, Amount accountsPayable) =>
        amount > -accountsPayable ? amount + accountsPayable : Amount.Zero;

    static IPersistenceContext UpdateUser(PayOutCommand command, TransactionId transactionId, IPersistenceContext context) =>
        context.UpdateItem<User>(user => UpdateUser(command, transactionId, user));

    static User UpdateUser(PayOutCommand command, TransactionId transactionId, User user) =>
        TryRemoveAccountNumber(command.Timestamp, command.AdministratorUserId, AddAudit(command, transactionId, user));

    static User AddAudit(PayOutCommand command, TransactionId transactionId, User user) =>
        user with { Audits = user.Audits.Add(CreateAudit(command, transactionId)) };

    static UserAudit CreateAudit(PayOutCommand command, TransactionId transactionId) =>
        UserAudit.Create(command.Timestamp, command.AdministratorUserId, UserAuditType.PayOut, transactionId);
}
