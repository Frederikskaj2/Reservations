using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.PlaceOwnerOrderFunctions;
using static Frederikskaj2.Reservations.Application.ReservationValidationFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class PlaceOwnerOrderHandler
{
    public static EitherAsync<Failure, OrderDetails> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, PlaceOwnerOrderCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), command.UserId)
        let today = dateProvider.GetDate(command.Timestamp)
        let existingReservations = GetReservations(context1.Items<Order>()).ToSeq()
        from _1 in ValidateOwnerReservations(options, today, existingReservations, command.Reservations)
        from orderId in CreateOrderId(contextFactory)
        let context2 = PlaceOwnerOrder(options, command, context1, orderId)
        from orderDetails in CreateOrderDetails(options, today, context2, orderId)
        from _2 in WriteContext(context2)
        select orderDetails;
}
