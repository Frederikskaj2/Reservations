using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class HistoryReservationDay
    {
        public int Id { get; set; }
        public LocalDate Date { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
    }
}