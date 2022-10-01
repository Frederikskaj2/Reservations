using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

class OrderReservation
{
    public ReservationId ReservationId { get; init; }

    public ResourceId ResourceId { get; init; }
    public Extent Extent { get; init; }
}
