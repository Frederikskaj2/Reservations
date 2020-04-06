using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class PayOutModel : OrderModel
    {
        public PayOutModel(string from, Uri fromUrl, string name, Uri url, int orderId, int amount)
            : base(from, fromUrl, name, url, orderId) => Amount = amount;

        public int Amount { get; }
    }
}