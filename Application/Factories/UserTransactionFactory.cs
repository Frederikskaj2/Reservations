using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.TransactionDescriptionFactory;

namespace Frederikskaj2.Reservations.Application;

static class UserTransactionFactory
{
    public static UserTransactions CreateUserTransactions(IFormatter formatter, UserFullName userFullName, IEnumerable<Transaction> transactions) =>
        new(userFullName.UserId, userFullName.FullName, transactions.Map(transaction => CreateUserTransaction(formatter, transaction)));

    static UserTransaction CreateUserTransaction(IFormatter formatter, Transaction transaction) =>
        new(
            transaction.TransactionId,
            transaction.Date,
            transaction.Activity,
            transaction.OrderId,
            CreateDescription(formatter, transaction.OrderId, transaction.Description),
            -(transaction.Amounts[Account.AccountsReceivable] + transaction.Amounts[Account.AccountsPayable]));
}
