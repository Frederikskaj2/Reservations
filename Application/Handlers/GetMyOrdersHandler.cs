using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.MyOrderFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetMyOrdersHandler
{
    public static EitherAsync<Failure, IEnumerable<MyOrder>> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, GetMyOrdersCommand command) =>
        GetMyOrders(contextFactory, dateProvider, options, command);

    static EitherAsync<Failure, IEnumerable<MyOrder>> GetMyOrders(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, GetMyOrdersCommand command) =>
        from context1 in ReadUserAndOrdersIncludingHistoryOrdersContext(CreateContext(contextFactory), command.UserId)
        let today = dateProvider.GetDate(command.Timestamp)
        from context2 in ReadLockBoxCodesContext(context1, today)
        from _ in WriteContext(context2)
        let sortedOrders = context2.Items<Order>().OrderBy(order => order.OrderId)
        select CreateMyOrders(options, today, context2.Item<LockBoxCodes>(), sortedOrders, context2.Item<User>());
}
