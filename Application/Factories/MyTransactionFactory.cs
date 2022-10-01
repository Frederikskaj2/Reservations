using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.PaymentFunctions;
using static Frederikskaj2.Reservations.Application.TransactionDescriptionFactory;

namespace Frederikskaj2.Reservations.Application;

static class MyTransactionFactory
{
    public static MyTransactions CreateMyTransactions(IFormatter formatter, OrderingOptions options, User user, IEnumerable<Transaction> transactions) =>
        new(transactions.Map(transaction => CreateMyTransaction(formatter, transaction)), GetPaymentInformation(options, user));

    static MyTransaction CreateMyTransaction(IFormatter formatter, Transaction transaction) =>
        new(
            transaction.Date,
            transaction.Activity,
            transaction.OrderId,
            CreateDescription(formatter, transaction.OrderId, transaction.Description),
            -(transaction.Amounts[Account.AccountsReceivable] + transaction.Amounts[Account.AccountsPayable]));
}
