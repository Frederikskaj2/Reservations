namespace Frederikskaj2.Reservations.Shared
{
    public class AccountAmount
    {
        public Account Account { get; set; }
        // Debit is positive, credit is negative.
        public int Amount { get; set; }
    }
}