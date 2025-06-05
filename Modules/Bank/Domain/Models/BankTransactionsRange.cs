using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record BankTransactionsRange(Option<DateRange> DateRange, Option<LocalDate> LatestImportStartDate);
