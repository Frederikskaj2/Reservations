using System;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using User = Frederikskaj2.Reservations.Server.Data.User;

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