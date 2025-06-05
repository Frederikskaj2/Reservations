using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record ImportBankTransactionsOutput(Seq<BankTransaction> Transactions, Option<LocalDate> LatestImportStartDate, Option<LocalDate> ImportStartDate);
