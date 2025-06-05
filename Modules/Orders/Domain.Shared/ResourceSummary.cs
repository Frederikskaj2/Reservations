using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record ResourceSummary(ResourceType ResourceType, int ReservationCount, int Nights, Amount Income);