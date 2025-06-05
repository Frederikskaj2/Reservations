using Frederikskaj2.Reservations.Users;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;

namespace Frederikskaj2.Reservations.Orders;

static class Reimburse
{
    public static ReimburseOutput ReimburseCore(ReimburseInput input) =>
        CreateOutput(input, CreateReimburseTransaction(input.Command, input.TransactionId));

    static Transaction CreateReimburseTransaction(ReimburseCommand command, TransactionId transactionId) =>
        new(
            transactionId,
            command.Date,
            command.AdministratorUserId,
            command.Timestamp,
            Activity.Reimburse,
            command.UserId,
            (TransactionDescription) new Reimbursement(command.AccountToDebit, command.Description),
            Reimburse(GetAccount(command.AccountToDebit), command.Amount));

    static Account GetAccount(IncomeAccount incomeAccount) =>
        incomeAccount switch
        {
            IncomeAccount.Rent => Account.Rent,
            IncomeAccount.Cleaning => Account.Cleaning,
            IncomeAccount.CancellationFees => Account.CancellationFees,
            IncomeAccount.Damages => Account.Damages,
            _ => throw new UnreachableException(),
        };

    static ReimburseOutput CreateOutput(ReimburseInput input, Transaction transaction) =>
        new(AddAudit(input.Command, input.User.AddTransaction(transaction), input.TransactionId), transaction);

    static User AddAudit(ReimburseCommand command, User user, TransactionId transactionId) =>
        user with
        {
            Audits = user.Audits.Add(UserAudit.Reimburse(command.Timestamp, command.AdministratorUserId, transactionId)),
        };
}
