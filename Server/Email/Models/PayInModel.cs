using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class PayInModel : OrderModel
    {
        public PayInModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, int amount, int missingAmount,
            string accountNumber)
            : base(from, fromUrl, name, url, orderId)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (missingAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(missingAmount));
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));

            Amount = amount;
            MissingAmount = missingAmount;
            AccountNumber = accountNumber;
        }

        public int Amount { get; }
        public int MissingAmount { get; }
        public string AccountNumber { get; }
    }
}