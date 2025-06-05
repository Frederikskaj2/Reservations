using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Orders;

record UpdatedReservation(ReservationIndex ReservationIndex, Extent Extent, ResourceId ResourceId, Price Price);
