using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record BankAccountImport(BankAccountId BankAccountId, LocalDate StartDate);
