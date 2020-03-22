using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class ReservedDay
    {
        public int Id { get; set; }
        public Reservation? Reservation { get; set; }
        public LocalDate Date { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
    }
}