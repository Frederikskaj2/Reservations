using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record ImportBankTransactionsInput(Seq<ImportBankTransaction> NewTransactions, Seq<BankTransaction> ExistingBankTransaction);
