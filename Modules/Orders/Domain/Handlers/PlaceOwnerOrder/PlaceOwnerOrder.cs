using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class PlaceOwnerOrder
{
    public static PlaceOwnerOrderOutput PlaceOwnerOrderCore(PlaceOwnerOrderInput input) =>
        new(
            AuditOrderPlacement(input.Command.Timestamp, input.User, input.OrderId),
            CreateOrder(input.Command, input.OrderId));

    static User AuditOrderPlacement(Instant timestamp, User user, OrderId orderId) =>
        user with { Audits = user.Audits.Add(UserAudit.CreateOwnerOrder(timestamp, user.UserId, orderId)) };

    static Order CreateOrder(PlaceOwnerOrderCommand command, OrderId orderId) =>
        new(
            orderId,
            command.UserId,
            GetFlags(command),
            command.Timestamp,
            new Owner(command.Description),
            CreateOwnerReservations(command),
            OrderAudit.PlaceOwnerOrder(command.Timestamp, command.UserId).Cons());

    static OrderFlags GetFlags(PlaceOwnerOrderCommand command) =>
        OrderFlags.IsOwnerOrder | (command.IsCleaningRequired ? OrderFlags.IsCleaningRequired : OrderFlags.None);

    static Seq<Reservation> CreateOwnerReservations(PlaceOwnerOrderCommand command) =>
        command.Reservations
            .Map(reservation =>
                new Reservation(
                    reservation.ResourceId,
                    ReservationStatus.Confirmed,
                    reservation.Extent,
                    None,
                    ReservationEmails.None))
            .ToSeq();
}
