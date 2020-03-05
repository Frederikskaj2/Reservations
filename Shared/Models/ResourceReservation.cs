using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ResourceReservation
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
        public ReservationStatus Status { get; set; }
    }
}