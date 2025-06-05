using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record DatedLockBoxCode(LocalDate Date, string Code);
