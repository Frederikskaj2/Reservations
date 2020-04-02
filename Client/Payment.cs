namespace Frederikskaj2.Reservations.Client
{
    public class Payment
    {
        public Payment(int orderId, int amount, bool isPayIn)
        {
            OrderId = orderId;
            Amount = amount;
            IsPayIn = isPayIn;
        }

        public int OrderId { get; }
        public int Amount { get; }
        public bool IsPayIn { get; }
    }
}
