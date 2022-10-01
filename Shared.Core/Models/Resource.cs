namespace Frederikskaj2.Reservations.Shared.Core;

public record Resource(ResourceId ResourceId, int Sequence, ResourceType Type, string Name);
