using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.UserFactory;

namespace Frederikskaj2.Reservations.Application;

static class OrderSummaryFactory
{
    public static IEnumerable<OrderSummary> CreateOrderSummaries(IEnumerable<Order> orders, IEnumerable<User> users) =>
        CreateOrderSummaries(orders, Prelude.toHashMap(users.Map(user => (user.UserId, user))));

    static IEnumerable<OrderSummary> CreateOrderSummaries(IEnumerable<Order> orders, HashMap<UserId, User> users) =>
        orders.Map(order => CreateOrderSummary(order, users));

    static OrderSummary CreateOrderSummary(Order order, HashMap<UserId, User> users) =>
        new(
            order.OrderId,
            order.User is not null ? OrderType.User : OrderType.Owner,
            order.CreatedTimestamp,
            CreateUserInformation(users[order.UserId]));
}
