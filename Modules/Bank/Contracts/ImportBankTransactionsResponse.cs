using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ImportBankTransactionsResponse(int Count, LocalDate? LatestImportStartDate);
