using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;

namespace Frederikskaj2.Reservations.Application;

static class PlaceOwnerOrderFunctions
{
    public static IPersistenceContext PlaceOwnerOrder(
        OrderingOptions options, PlaceOwnerOrderCommand command, IPersistenceContext context, OrderId orderId) =>
        ScheduleCleaning(options, AddOrder(context, CreateOrder(command, orderId)));

    static IPersistenceContext AddOrder(IPersistenceContext context, Order order) =>
        context
            .CreateItem(order, o => Order.GetId(o.OrderId))
            .UpdateItem<User>(user => user with
            {
                Audits = user.Audits.Add(new UserAudit(order.CreatedTimestamp, user.UserId, UserAuditType.CreateOwnerOrder, order.OrderId))
            });

    static Order CreateOrder(PlaceOwnerOrderCommand command, OrderId orderId) =>
        new(
            orderId,
            command.UserId,
            GetFlags(command),
            command.Timestamp,
            null,
            new OwnerOrder(command.Description),
            CreateOwnerReservations(command),
            CreateOrderAudit(command).Cons());

    static OrderFlags GetFlags(PlaceOwnerOrderCommand command) =>
        OrderFlags.IsOwnerOrder | (command.IsCleaningRequired ? OrderFlags.IsCleaningRequired : OrderFlags.None);

    static Seq<Reservation> CreateOwnerReservations(PlaceOwnerOrderCommand command) =>
        command.Reservations
            .Map(reservation =>
                new Reservation(
                    reservation.ResourceId,
                    ReservationStatus.Confirmed,
                    reservation.Extent,
                    null,
                    ReservationEmails.None,
                    null))
            .ToSeq();

    static OrderAudit CreateOrderAudit(PlaceOwnerOrderCommand command) =>
        new(command.Timestamp, command.UserId, OrderAuditType.PlaceOrder);
}
