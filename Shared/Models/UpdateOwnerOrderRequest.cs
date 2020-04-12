using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared
{
    public class UpdateOwnerOrderRequest
    {
        public HashSet<int> CancelledReservations { get; } = new HashSet<int>();
    }
}
