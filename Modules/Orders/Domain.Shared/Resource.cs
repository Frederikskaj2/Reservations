namespace Frederikskaj2.Reservations.Orders;

public record Resource(ResourceId ResourceId, int Sequence, ResourceType Type, string Name);
