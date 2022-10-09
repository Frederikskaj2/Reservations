using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.AccountNumberFunctions;
using static Frederikskaj2.Reservations.Application.CancelReservationsFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.OrderDetailsFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateUserOrderHandler
{
    public static EitherAsync<Failure, OrderDetails> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        UpdateUserOrderCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), command.UserId)
        let context2 = UpdateAccountNumber(context1, command.Timestamp, command.AdministratorUserId, command.AccountNumber)
        from order in GetUserOrder(context2, command.OrderId)
        let today = dateProvider.GetDate(command.Timestamp)
        from context3 in TryCancelReservations(options, command.Timestamp, command.UserId, command.CancelledReservations, command.WaiveFee, true, today, context2, order)
        let tuple = TryAllowCancellationWithoutFee(
            options, command.Timestamp, command.AdministratorUserId, command.AllowCancellationWithoutFee, order, context3)
        from contextWithPossiblyDeletedUser in TryDeleteUser(emailService, tuple.Context, command.Timestamp, order.UserId)
        from _1 in WriteContext(contextWithPossiblyDeletedUser)
        from _2 in SendReservationsCancelledEmail(emailService, command.OrderId, command.CancelledReservations, tuple.Context)
        from _3 in SendOrdersConfirmedEmail(emailService, context1, tuple.Context)
        from _4 in SendNoFeeCancellationIsAllowed(
            emailService, options.CancellationWithoutFeeDuration, command.OrderId, tuple.Context.Item<User>(), tuple.IsCancellationWithoutFeeAllowed)
        from orderDetails in CreateOrderDetails(options, today, tuple.Context, command.OrderId)
        select orderDetails;
}
