using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Posting
    {
        public int TransactionId { get; set; }
        public LocalDate Date { get; set; }
        public int OrderId { get; set; }
        public Account Account { get; set; }
        // Debit is positive, credit is negative.
        public int Amount { get; set; }
    }
}
