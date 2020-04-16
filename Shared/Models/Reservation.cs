using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public ReservationStatus Status { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public Price? Price { get; set; }
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
        public bool CanBeCancelled { get; set; }
        public List<DatedKeyCode>? KeyCodes { get; set; }
    }
}