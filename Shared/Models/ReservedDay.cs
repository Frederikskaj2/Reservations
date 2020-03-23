using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservedDay
    {
        public LocalDate Date { get; set; }
        public int ResourceId { get; set; }
        public bool IsMyReservation { get; set; }
    }
}