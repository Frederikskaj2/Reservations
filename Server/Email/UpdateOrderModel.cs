using System;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class UpdateOrderModel
    {
        public UpdateOrderModel(User user, Order order, Uri url)
        {
            User = user;
            Order = order;
            Url = url;
        }

        public User User { get; }
        public Order Order { get; }
        public Uri Url { get; }
    }
}