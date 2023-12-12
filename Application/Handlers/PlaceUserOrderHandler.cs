using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

public static class PlaceUserOrderHandler
{
    public static EitherAsync<Failure, MyOrder> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        PlaceUserOrderCommand command) =>
        from context1 in DatabaseFunctions.ReadUserAndAllOrdersContext(DatabaseFunctions.CreateContext(contextFactory), command.UserId)
        from _1 in PlaceUserOrderFunctions.ValidateUserCanPlaceOrder(context1.Item<User>())
        let today = dateProvider.GetDate(command.Timestamp)
        let existingReservations = OrderFunctions.GetReservations(context1.Items<Order>()).ToSeq()
        from _2 in ReservationValidationFunctions.ValidateUserReservationsWithOwnerPolicies(options, today, existingReservations, command.Reservations)
        from context2 in LockBoxCodeFunctions.ReadLockBoxCodesContext(context1, today)
        from orderId in OrderFunctions.CreateOrderId(contextFactory)
        from transactionId in TransactionFunctions.CreateTransactionId(contextFactory)
        let context3 = PlaceUserOrderFunctions.PlaceUserOrder(options, dateProvider.Holidays, command, today, context2, orderId, transactionId)
        let user = context3.Item<User>()
        let order = context3.Item<Order>(Order.GetId(orderId))
        from _3 in DatabaseFunctions.WriteContext(context3)
        let payment = PaymentFunctions.GetPaymentInformation(options, user)
        from _4 in EmailFunctions.SendOrderReceivedEmail(emailService, user, order, payment)
        from _5 in EmailFunctions.SendNewOrderEmail(context3, emailService, orderId)
        from _6 in EmailFunctions.SendOrdersConfirmedEmail(emailService, context1, context3)
        select MyOrderFactory.CreateMyOrder(options, today, context3.Item<LockBoxCodes>(), order, user, payment);
}
