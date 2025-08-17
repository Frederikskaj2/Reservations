using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record ImportBankTransactionsOutput(
    Seq<BankTransaction> Transactions,
    Option<DateRange> DateRange,
    Option<LocalDate> LatestImportStartDate,
    Option<Import> Import);
