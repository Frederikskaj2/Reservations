using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UserOrderFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetUserOrdersHandler
{
    public static EitherAsync<Failure, IEnumerable<Shared.Core.Order>> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options) =>
        GetUserOrders(options, CreateContext(contextFactory), dateProvider.Today);

    static EitherAsync<Failure, IEnumerable<Shared.Core.Order>> GetUserOrders(OrderingOptions options, IPersistenceContext context, LocalDate today) =>
        from orders in ReadUserOrders(context)
        from users in ReadUsers(context, orders.Select(order => order.UserId).Distinct())
        let userDictionary = users.ToDictionary(user => user.UserId)
        select orders.Select(order => CreateUserOrder(options, today, order, userDictionary));
}
