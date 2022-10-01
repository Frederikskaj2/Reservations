using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record SettlementNeededEmail(
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date);
