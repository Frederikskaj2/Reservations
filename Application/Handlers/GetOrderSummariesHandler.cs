using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.OrderSummaryFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetOrderSummariesHandler
{
    public static EitherAsync<Failure, IEnumerable<OrderSummary>> Handle(IPersistenceContextFactory contextFactory) =>
        Handle(CreateContext(contextFactory));

    static EitherAsync<Failure, IEnumerable<OrderSummary>> Handle(IPersistenceContext context) =>
        from orders in ReadOrders(context)
        from users in ReadUsers(context, orders.Map(order => order.UserId).Distinct())
        select CreateOrderSummaries(orders, users);
}
