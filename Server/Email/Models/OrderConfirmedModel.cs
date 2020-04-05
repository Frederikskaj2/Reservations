using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class OrderConfirmedModel : OrderModel
    {
        public OrderConfirmedModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, int amount, int excessAmount)
            : base(from, fromUrl, name, url, orderId)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (excessAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Amount = amount;
            ExcessAmount = excessAmount;
        }

        public int Amount { get; }
        public int ExcessAmount { get; }
    }
}