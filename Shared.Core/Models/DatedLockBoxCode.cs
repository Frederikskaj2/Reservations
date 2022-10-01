using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record DatedLockBoxCode(LocalDate Date, string Code);
