using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class OrderModel : EmailWithUrlModel
    {
        public OrderModel(string from, Uri fromUrl, string name, Uri url, int orderId)
            : base(from, fromUrl, name, url) => OrderId = orderId;

        public int OrderId { get; }
    }
}