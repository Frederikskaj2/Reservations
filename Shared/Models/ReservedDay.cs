using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservedDay
    {
        public int Id { get; set; }
        public Reservation? Reservation { get; set; }
        public LocalDate Date { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }

        [NotMapped]
        public bool IsMyReservation { get; set; }

        public override string ToString() => Date.ToString();
    }
}