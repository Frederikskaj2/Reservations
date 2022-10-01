using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Net;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.MyOrderFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class GetMyOrderHandler
{
    public static EitherAsync<Failure, MyOrder> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, GetMyOrderCommand command) =>
        GetMyOrder(dateProvider, options, contextFactory, command);

    static EitherAsync<Failure, MyOrder> GetMyOrder(
        IDateProvider dateProvider, OrderingOptions options, IPersistenceContextFactory contextFactory, GetMyOrderCommand command) =>
        from context1 in ReadOrderContext(CreateContext(contextFactory), command.OrderId)
        from _1 in ValidateOrderOwnership(command.UserId, context1.Item<Order>())
        from context2 in ReadUserContext(context1, command.UserId)
        let today = dateProvider.GetDate(command.Timestamp)
        from context3 in ReadLockBoxCodesContext(context2, today)
        from _2 in WriteContext(context3)
        select CreateMyOrder(options, today, context3.Item<LockBoxCodes>(), context3.Item<Order>(), context3.Item<User>());

    static EitherAsync<Failure, Unit> ValidateOrderOwnership(UserId userId, Order order) =>
        order.UserId == userId ? unit : Failure.New(HttpStatusCode.NotFound, $"Order {order.OrderId} does not belong to user {userId}.");
}
