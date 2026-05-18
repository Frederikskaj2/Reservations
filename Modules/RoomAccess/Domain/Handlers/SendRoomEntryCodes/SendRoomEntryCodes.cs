using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

static class SendRoomEntryCodes
{
    public static SendRoomEntryCodesOutput SendRoomEntryCodesCore(OrderingOptions options, SendRoomEntryCodesInput input) =>
        SendRoomEntryCodesCore(input, GetUpcomingReservations(input.Orders, input.Command.Date.Plus(options.RevealEntryCodeBeforeReservationStart)));

    static Seq<ReservationWithOrder> GetUpcomingReservations(Seq<Order> orders, LocalDate latestDate) =>
        orders
            .Bind(
                order => order.Reservations
                    .Map(reservation => new ReservationWithOrder(reservation, order))
                    .Filter(
                        reservationWithOrder =>
                            reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed &&
                            !reservationWithOrder.Reservation.SentEmails.HasFlag(ReservationEmails.RoomEntryCode) &&
                            reservationWithOrder.Reservation.Extent.Date < latestDate))
            .ToSeq();

    static SendRoomEntryCodesOutput SendRoomEntryCodesCore(SendRoomEntryCodesInput input, Seq<ReservationWithOrder> upcomingReservations) =>
        SendRoomEntryCodesCore(upcomingReservations, toHashMap(input.Users.Map(user => (user.UserId, user))));

    static SendRoomEntryCodesOutput SendRoomEntryCodesCore(Seq<ReservationWithOrder> upcomingReservations, HashMap<UserId, UserExcerpt> users) =>
        new(
            OrdersFunctions.SetReservationsEmailFlag(upcomingReservations, ReservationEmails.RoomEntryCode),
            upcomingReservations.Map(reservation => CreateEmail(users, reservation)).ToSeq());

    static RoomEntryCodeEmail CreateEmail(HashMap<UserId, UserExcerpt> users, ReservationWithOrder reservationWithOrder) =>
        CreateEmail(reservationWithOrder, GetUser(users, reservationWithOrder.Order.UserId));

    static UserExcerpt GetUser(HashMap<UserId, UserExcerpt> users, UserId userId) =>
        users.Find(userId).Case switch
        {
            UserExcerpt user => user,
            _ => throw new UnreachableException(),
        };

    static RoomEntryCodeEmail CreateEmail(ReservationWithOrder reservationWithOrder, UserExcerpt user) =>
        new(
            user.Email(),
            user.FullName,
            reservationWithOrder.Order.OrderId,
            reservationWithOrder.Reservation.ResourceId,
            reservationWithOrder.Reservation.Extent.Date,
            reservationWithOrder.Reservation.EntryCode!.Value);
}
