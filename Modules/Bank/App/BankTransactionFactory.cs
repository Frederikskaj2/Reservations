using LanguageExt;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

static class BankTransactionFactory
{
    public static IEnumerable<BankTransactionDto> CreateBankTransactions(Seq<BankTransaction> transactions) =>
        transactions.Map(CreateBankTransaction);

    public static BankTransactionDto CreateBankTransaction(BankTransaction transaction) =>
        new(
            transaction.BankTransactionId,
            transaction.Date,
            transaction.Text,
            transaction.Amount,
            transaction.Balance,
            transaction.Status);
}
