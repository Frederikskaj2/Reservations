using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record ImportBankTransactionsInput(
    ImportBankTransactionsCommand Command,
    Seq<ImportBankTransaction> NewTransactions,
    Seq<BankTransaction> ExistingBankTransaction);
