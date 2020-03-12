using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public int Id { get; set; }
        public Order? Order { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public ReservationStatus Status { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public List<ReservedDay>? Days { get; set; }
        public Price? Price { get; set; }

        [NotMapped]
        public LocalDate Date { get; set; }

        [NotMapped]
        public int DurationInDays { get; set; }

        [NotMapped]
        public bool CanBeCancelled { get; set; }

        public override string ToString() => $"{Resource?.Name ?? ResourceId.ToString()} {Date}+{DurationInDays}";
    }
}