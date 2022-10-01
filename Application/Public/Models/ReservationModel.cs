using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record ReservationModel(ResourceId ResourceId, ResourceType ResourceType, Extent Extent);
