using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public record Holiday(LocalDate Date, bool IsOnlyBankHoliday);
