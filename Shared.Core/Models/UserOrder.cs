using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record UserOrder(
    string? AccountNumber,
    Instant? NoFeeCancellationIsAllowedBefore,
    IEnumerable<LineItem> AdditionalLineItems);
