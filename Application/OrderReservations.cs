using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

class OrderReservations
{
    public IEnumerable<OrderReservation> Reservations { get; init; } = null!;
}