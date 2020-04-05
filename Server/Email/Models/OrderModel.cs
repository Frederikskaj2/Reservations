using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public abstract class OrderModel : EmailWithUrlModel
    {
        protected OrderModel(string from, Uri fromUrl, string name, Uri url, int orderId) : base(
            name, url, from, fromUrl) => OrderId = orderId;

        public int OrderId { get; }
    }
}