using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class OrderReceivedModel : OrderModel
    {
        public OrderReceivedModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, int balance, int amount, string accountNumber)
            : base(from, fromUrl, name, url, orderId)
        {
            if (balance < 0)
                throw new ArgumentOutOfRangeException(nameof(balance));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));

            Balance = balance;
            Amount = amount;
            AccountNumber = accountNumber;
        }

        public int Balance { get; }
        public int Amount { get; }
        public string AccountNumber { get; }
    }
}