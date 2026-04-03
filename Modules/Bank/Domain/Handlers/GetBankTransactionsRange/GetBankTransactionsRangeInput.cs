using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record GetBankTransactionsRangeInput(
    GetBankTransactionsRangeQuery Query,
    Option<BankTransaction> EarliestTransaction,
    Option<BankTransaction> LatestTransaction,
    Option<Import> Import);
