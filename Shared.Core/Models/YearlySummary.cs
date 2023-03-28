using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record YearlySummary(int Year, IEnumerable<ResourceSummary> ResourceSummaries);
