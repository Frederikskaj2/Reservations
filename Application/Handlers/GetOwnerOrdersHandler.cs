using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.OwnerOrderFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetOwnerOrdersHandler
{
    public static EitherAsync<Failure, IEnumerable<Shared.Core.Order>> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, Instant timestamp) =>
        from context1 in ReadOwnerOrdersContext(CreateContext(contextFactory))
        let today = dateProvider.GetDate(timestamp)
        from context2 in MakeHistoryOwnerOrders(dateProvider, today, context1)
        let orders = context2.Items<Order>()
        from users in ReadUsers(context2, orders.Map(order => order.UserId).Distinct())
        select OwnerOrderFactory.CreateOwnerOrders(today, orders, users);
}
