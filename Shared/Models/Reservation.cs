using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
        public ReservationStatus Status { get; set; }

        [NotMapped]
        public bool IsMyReservation { get; set; }

        [NotMapped]
        public Price? Price { get; set; }

        public override string ToString() => $"#{Id}: R{ResourceId} {Date}+{DurationInDays}";
    }
}