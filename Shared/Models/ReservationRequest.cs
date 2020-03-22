using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservationRequest
    {
        public int ResourceId { get; set; }
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
    }
}