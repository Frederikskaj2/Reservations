namespace Frederikskaj2.Reservations.LockBox;

public record Resource(ResourceId ResourceId, int Sequence, ResourceType Type, string Name);
