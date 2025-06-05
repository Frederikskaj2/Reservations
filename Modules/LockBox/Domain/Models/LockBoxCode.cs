using NodaTime;

namespace Frederikskaj2.Reservations.LockBox;

public record LockBoxCode(ResourceId ResourceId, LocalDate Date, string Code);
