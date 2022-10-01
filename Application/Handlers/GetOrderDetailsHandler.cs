using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Application.OwnerOrderFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetOrderDetailsHandler
{
    public static EitherAsync<Failure, OrderDetails> Handle(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, GetOrderCommand command) =>
        GetOrderDetails(options, dateProvider, command.OrderId, dateProvider.GetDate(command.Timestamp), CreateContext(contextFactory));

    static EitherAsync<Failure, OrderDetails> GetOrderDetails(OrderingOptions options, IDateProvider dateProvider, OrderId orderId, LocalDate today, IPersistenceContext context) =>
        from context1 in ReadOrderAndUserContext(context, orderId)
        from context2 in MakeHistoryOwnerOrder(dateProvider, today, context1)
        from orderDetails in CreateOrderDetails(options, today, context2, orderId)
        select orderDetails;
}
