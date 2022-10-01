using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

static class PersistenceContextExtensions
{
    public static Order Order(this IPersistenceContext context, OrderId orderId) =>
        context.Item<Order>(Application.Order.GetId(orderId));

    public static Option<Order> OrderOption(this IPersistenceContext context, OrderId orderId) =>
        context.ItemOption<Order>(Application.Order.GetId(orderId));

}
