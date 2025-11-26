using Frederikskaj2.Reservations.Users;
using System;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;

namespace Frederikskaj2.Reservations.Orders;

static class Reimburse
{
    public static ReimburseOutput ReimburseCore(ReimburseInput input) =>
        CreateOutput(input, CreateReimburseTransaction(input));

    static Transaction CreateReimburseTransaction(ReimburseInput input) =>
        new(
            input.TransactionId,
            input.Command.Date,
            input.Command.AdministratorUserId,
            input.Command.Timestamp,
            Activity.Reimburse,
            input.Command.UserId,
            (TransactionDescription) new Reimbursement(input.Command.AccountToDebit, input.Command.Description),
            Reimburse(
                GetAccount(input.Command.AccountToDebit),
                input.Command.Amount,
                GetAccountPayableAmount(input.User.Accounts, input.Command.Amount),
                GetAccountReceivableAmount(input.User.Accounts, input.Command.Amount)));

    static Account GetAccount(IncomeAccount incomeAccount) =>
        incomeAccount switch
        {
            IncomeAccount.Rent => Account.Rent,
            IncomeAccount.Cleaning => Account.Cleaning,
            IncomeAccount.CancellationFees => Account.CancellationFees,
            IncomeAccount.Damages => Account.Damages,
            _ => throw new UnreachableException(),
        };

    static Amount GetAccountPayableAmount(AccountAmounts accounts, Amount amount) =>
        accounts[Account.AccountsPayable] < Amount.Zero
            ? amount
            : -Amount.Min(accounts[Account.AccountsReceivable] - amount, Amount.Zero);

    static Amount GetAccountReceivableAmount(AccountAmounts accounts, Amount amount) =>
        accounts[Account.AccountsReceivable] > Amount.Zero
            ? Amount.Min(accounts[Account.AccountsReceivable], amount)
            : Amount.Zero;

    static ReimburseOutput CreateOutput(ReimburseInput input, Transaction transaction) =>
        new(AddAudit(input.Command, input.User.AddTransaction(transaction), input.TransactionId), transaction);

    static User AddAudit(ReimburseCommand command, User user, TransactionId transactionId) =>
        user with
        {
            Audits = user.Audits.Add(UserAudit.Reimburse(command.Timestamp, command.AdministratorUserId, transactionId)),
        };
}
