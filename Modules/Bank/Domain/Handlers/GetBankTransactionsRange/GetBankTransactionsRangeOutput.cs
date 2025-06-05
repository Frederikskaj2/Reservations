using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record GetBankTransactionsRangeOutput(Option<DateRange> DateRange, Option<LocalDate> LatestImportStartDate);
