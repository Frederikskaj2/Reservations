using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class AccountBalance
    {
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        public Account Account { get; set; }

        // Debit is positive.
        public int Amount { get; set; }

    }
}
