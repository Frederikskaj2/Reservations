namespace Frederikskaj2.Reservations.Shared.Core;

public record ResourceSummary(ResourceType ResourceType, int ReservationCount, int Nights, Amount Income);
