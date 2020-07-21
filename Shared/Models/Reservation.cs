using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A public setter is required for serialization.")]
        public List<DatedLockBoxCode>? LockBoxCodes { get; set; }
    }
}