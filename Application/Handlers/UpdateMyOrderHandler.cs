using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.AccountNumberFunctions;
using static Frederikskaj2.Reservations.Application.CancelReservationsFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.FeeFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.MyOrderFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateMyOrderHandler
{
    public static EitherAsync<Failure, UpdateMyOrderResult> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        UpdateMyOrderCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), command.UserId)
        let context2 = UpdateAccountNumber(context1, command.Timestamp, command.UserId, command.AccountNumber)
        let today = dateProvider.GetDate(command.Timestamp)
        from context3 in ReadLockBoxCodesContext(context2, today)
        from order in GetUserOrder(context3, command.OrderId)
        from waiveFee in ValidateUserWaivingFee(order, command.Timestamp, command.WaiveFee)
        let alwaysAllowCancellation = command.Timestamp <= order.User!.NoFeeCancellationIsAllowedBefore
        from context4 in TryCancelReservations(
            options, command.Timestamp, command.UserId, command.CancelledReservations, waiveFee, alwaysAllowCancellation, today, context3, order)
        let user = context4.Item<User>()
        from context5 in TryDeleteUser(emailService, context4, command.Timestamp, order.UserId)
        from _1 in WriteContext(context5)
        let updatedOrder = context5.Item<Order>(Order.GetId(command.OrderId))
        from _3 in SendReservationsCancelledEmail(emailService, command.OrderId, command.CancelledReservations, context5)
        from _4 in SendOrdersConfirmedEmail(emailService, context1, context5)
        let isUserDeleted = context5.Item<User>().Flags.HasFlag(UserFlags.IsDeleted)
        select new UpdateMyOrderResult(CreateMyOrder(options, today, context4.Item<LockBoxCodes>(), updatedOrder, user), isUserDeleted);
}
