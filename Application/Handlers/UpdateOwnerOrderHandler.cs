using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.CancelReservationsFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.OwnerOrderFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateOwnerOrderHandler
{
    public static EitherAsync<Failure, OrderDetails> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, OrderingOptions options, UpdateOwnerOrderCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), command.UserId)
        from order in GetOwnerOrder(context1, command.OrderId)
        from context2 in UpdateOwnerOrder(command, context1)
        let today = dateProvider.GetDate(command.Timestamp)
        from context3 in TryCancelOwnerReservations(options, command.Timestamp, command.UserId, command.CancelledReservations, today, context2, order)
        let context4 = ScheduleCleaning(options, context3)
        let context5 = TryMakeHistoryOwnerOrders(dateProvider, command.Timestamp, today, GetCancelledOrderId(command), context4)
        from result in CreateOrderDetails(options, today, context5, command.OrderId)
        from _ in WriteContext(context5)
        select result;

    static Option<OrderId> GetCancelledOrderId(UpdateOwnerOrderCommand command) =>
        command.CancelledReservations.Count > 0 ? command.OrderId : None;
}
