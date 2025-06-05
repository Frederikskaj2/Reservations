using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record GetBankTransactionsRangeResponse(DateRange? DateRange, LocalDate? LatestImportStartDate);
