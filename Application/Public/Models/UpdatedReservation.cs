using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

record UpdatedReservation(ReservationIndex ReservationIndex, Extent Extent, ResourceId ResourceId, Price Price);
