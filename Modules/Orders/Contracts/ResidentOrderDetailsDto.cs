using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record ResidentOrderDetailsDto(
    string? AccountNumber,
    Instant? NoFeeCancellationIsAllowedBefore,
    IEnumerable<LineItem> AdditionalLineItems);