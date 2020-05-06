using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class OrderReceivedModel : OrderModel
    {
        public OrderReceivedModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, int prepaidAmount, int amount, string accountNumber)
            : base(from, fromUrl, name, url, orderId)
        {
            if (prepaidAmount < 0 || prepaidAmount > amount)
                throw new ArgumentOutOfRangeException(nameof(prepaidAmount));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));

            PrepaidAmount = prepaidAmount;
            Amount = amount;
            AccountNumber = accountNumber;
        }

        public int PrepaidAmount { get; }
        public int Amount { get; }
        public string AccountNumber { get; }
    }
}