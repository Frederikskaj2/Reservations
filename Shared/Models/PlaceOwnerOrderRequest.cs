using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared
{
    public class PlaceOwnerOrderRequest
    {
        public List<ReservationRequest> Reservations { get; } = new List<ReservationRequest>();
        public bool IsCleaningRequired { get; set; }
    }
}