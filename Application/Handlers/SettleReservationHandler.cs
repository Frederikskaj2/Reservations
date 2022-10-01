using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.HistoryOrderFunctions;
using static Frederikskaj2.Reservations.Application.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.SettleReservationFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SettleReservationHandler
{
    public static EitherAsync<Failure, OrderDetails> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        SettleReservationCommand command) =>
        from context1 in ReadUserAndOrdersContext(CreateContext(contextFactory), command.UserId)
        from order in GetUserOrder(context1, command.OrderId)
        from reservation in GetReservation(options, dateProvider, command.Timestamp, order, command.ReservationId, command.Damages)
        let today = dateProvider.GetDate(command.Timestamp)
        from context2 in SettleReservation(command, today, context1, order, reservation)
        let context3 = TryMakeHistoryOrder(command.Timestamp, command.AdministratorUserId, command.OrderId, context2)
        let user = context3.Item<User>()
        from context4 in TryDeleteUser(emailService, context3, command.Timestamp, order.UserId)
        from orderDetails in CreateOrderDetails(options, today, context4, command.OrderId)
        from _1 in DatabaseFunctions.WriteContext(context4)
        let updateOrder = context4.Order(command.OrderId)
        from _2 in SendReservationSettledEmail(emailService, user, command.OrderId, reservation, command.Damages, command.Description)
        from _3 in SendOrdersConfirmedEmail(emailService, context1, context4)
        select orderDetails;
}
