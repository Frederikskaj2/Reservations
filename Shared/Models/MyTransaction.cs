using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class MyTransaction
    {
        public Instant Timestamp { get; set; }
        public TransactionType Type { get; set; }
        public User? User { get; set; }
        public int? OrderId { get; set; }
        public int? ResourceId { get; set; }
        public LocalDate? ReservationDate { get; set; }
        public string? Comment { get; set; }
        public decimal Amount { get; set; }
    }
}
