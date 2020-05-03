using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class Payment
    {
        public Payment(int orderId, LocalDate date, int amount, bool isPayIn)
        {
            OrderId = orderId;
            Date = date;
            Amount = amount;
            IsPayIn = isPayIn;
        }

        public int OrderId { get; }
        public LocalDate Date { get; }
        public int Amount { get; }
        public bool IsPayIn { get; }
    }
}
