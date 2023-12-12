using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.MyOrderFactory;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.PaymentFunctions;
using static Frederikskaj2.Reservations.Application.PlaceUserOrderFunctions;
using static Frederikskaj2.Reservations.Application.ReservationValidationFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class PlaceMyOrderHandler
{
    public static EitherAsync<Failure, MyOrder> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        PlaceMyOrderCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), command.UserId)
        from _1 in ValidateUserCanPlaceOrder(context1.Item<User>())
        let today = dateProvider.GetDate(command.Timestamp)
        let existingReservations = GetReservations(context1.Items<Order>()).ToSeq()
        from _2 in ValidateUserReservations(options, dateProvider.Holidays, today, existingReservations, command.Reservations)
        from context2 in ReadLockBoxCodesContext(context1, today)
        from orderId in CreateOrderId(contextFactory)
        from transactionId in CreateTransactionId(contextFactory)
        let context3 = PlaceUserOrder(
            options,
            dateProvider.Holidays,
            new(command.Timestamp, command.UserId, command.UserId, command.FullName, command.Phone, command.ApartmentId, command.AccountNumber, command.Reservations),
            today,
            context2,
            orderId,
            transactionId)
        let user = context3.Item<User>()
        let order = context3.Item<Order>(Order.GetId(orderId))
        from _3 in WriteContext(context3)
        let payment = GetPaymentInformation(options, user)
        from _4 in SendOrderReceivedEmail(emailService, user, order, payment)
        from _5 in SendNewOrderEmail(context3, emailService, orderId)
        from _6 in SendOrdersConfirmedEmail(emailService, context1, context3)
        select CreateMyOrder(options, today, context3.Item<LockBoxCodes>(), order, user, payment);
}
