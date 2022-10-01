using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.DebtorFactory;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;

namespace Frederikskaj2.Reservations.Application;

static class PayInFunctions
{
    public static EitherAsync<Failure, IEnumerable<Debtor>> GetDebtors(IPersistenceContext context) =>
        from users in MapReadError(context.Untracked.ReadItems(
            context.Query<User>().Where(user => !user.Flags.HasFlag(UserFlags.IsDeleted) && user.Roles.HasFlag(Roles.Resident) && user.ApartmentId != null)))
        select CreateDebtors(users);

    public static IPersistenceContext PayIn(OrderingOptions options, PayInCommand command, IPersistenceContext context, TransactionId transactionId) =>
        ScheduleCleaning(
            options,
            ApplyDebitToOrders(
                command.Timestamp,
                command.AdministratorUserId,
                UpdateUser(
                    command,
                    transactionId,
                    AddUserTransaction(
                        context,
                        CreatePayInTransaction(command, context.Item<User>().UserId, transactionId, GetExcessAmount(context, command.Amount)))),
                transactionId));

    static Amount GetExcessAmount(IPersistenceContext context, Amount amount) =>
        GetExcessAmount(amount, context.Item<User>().Accounts[Account.AccountsReceivable]);

    static Amount GetExcessAmount(Amount amount, Amount accountsReceivable) =>
        amount > accountsReceivable ? amount - accountsReceivable : Amount.Zero;

    static IPersistenceContext UpdateUser(PayInCommand command, TransactionId transactionId, IPersistenceContext context) =>
        context.UpdateItem<User>(user => AddAudit(command, transactionId, user));

    static User AddAudit(PayInCommand command, TransactionId transactionId, User user) =>
        user with { Audits = user.Audits.Add(CreateAudit(command, transactionId)) };

    static UserAudit CreateAudit(PayInCommand command, TransactionId transactionId) =>
        UserAudit.Create(command.Timestamp, command.AdministratorUserId, UserAuditType.PayIn, transactionId);
}
