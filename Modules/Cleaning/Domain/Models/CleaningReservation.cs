using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;

namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningReservation(ResourceId ResourceId, Extent Extent, bool IsCleaningRequired);
