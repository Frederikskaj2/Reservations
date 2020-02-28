using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class ResourceReservation
    {
        public Resource? Resource { get; set; }
        public DateTime Date { get; set; }
        public int DurationInDays { get; set; }
        public ReservationStatus Status { get; set; }
    }
}