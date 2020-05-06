using System;
using System.Linq;
using Frederikskaj2.Reservations.Shared;
using Order = Frederikskaj2.Reservations.Server.Data.Order;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal static class OrderExtensions
    {
        public static int Balance(this Order order, Account account)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            return order.Transactions.Sum(transaction => transaction.Amount(account));
        }
    }
}