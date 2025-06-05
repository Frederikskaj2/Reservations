using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record YearlySummary(int Year, IEnumerable<ResourceSummary> ResourceSummaries);