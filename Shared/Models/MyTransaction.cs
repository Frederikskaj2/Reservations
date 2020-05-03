using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class MyTransaction
    {
        public LocalDate Date { get; set; }
        public TransactionType Type { get; set; }
        public int? OrderId { get; set; }
        public int? ResourceId { get; set; }
        public LocalDate? ReservationDate { get; set; }
        public string? Comment { get; set; }
        public int Amount { get; set; }
    }
}
