using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class HistoryReservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ResourceId { get; set; }
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
        public int ApartmentId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}