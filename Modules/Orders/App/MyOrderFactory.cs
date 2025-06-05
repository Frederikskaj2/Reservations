using Frederikskaj2.Reservations.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Orders;

static class MyOrderFactory
{
    public static Seq<MyOrderDto> CreateMyOrders(Seq<ResidentOrder> orders) =>
        orders.Map(CreateMyOrder);

    public static MyOrderDto CreateMyOrder(ResidentOrder order) =>
        new(
            order.OrderId,
            order.CreatedTimestamp,
            CreateReservations(order.Reservations),
            order.IsHistoryOrder,
            order.CanBeEdited,
            order.Price,
            order.NoFeeCancellationIsAllowedBefore.ToNullable(),
            order.PaymentInformation.ToNullableReference(),
            order.AdditionalLineItems,
            CreateUserIdentity(order.User));

    static Seq<MyReservationDto> CreateReservations(Seq<ResidentReservation> reservations) =>
        reservations.Map(reservation =>
            new MyReservationDto(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                reservation.CanBeCancelled,
                reservation.LockBoxCodes));
}
