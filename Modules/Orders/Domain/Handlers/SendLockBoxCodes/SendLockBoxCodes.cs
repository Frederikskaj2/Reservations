using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.OrdersLockBoxCodesFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class SendLockBoxCodes
{
    public static SendLockBoxCodesOutput SendLockBoxCodesCore(OrderingOptions options, SendLockBoxCodesInput input) =>
        SendLockBoxCodesCore(
            options,
            input,
            GetUpcomingReservations(input.Orders, input.Command.Date.Plus(options.RevealLockBoxCodeBeforeReservationStart)));

    static Seq<ReservationWithOrder> GetUpcomingReservations(Seq<Order> orders, LocalDate latestDate) =>
        orders.Bind(
                order => order.Reservations
                    .Map((index, reservation) => new ReservationWithOrder(reservation, order, index))
                    .Filter(
                        reservationWithOrder =>
                            reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed &&
                            !reservationWithOrder.Reservation.SentEmails.HasFlag(ReservationEmails.LockBoxCode) &&
                            reservationWithOrder.Reservation.Extent.Date < latestDate))
            .ToSeq();

    static SendLockBoxCodesOutput SendLockBoxCodesCore(OrderingOptions options, SendLockBoxCodesInput input, Seq<ReservationWithOrder> upcomingReservations) =>
        SendLockBoxCodesCore(
            upcomingReservations,
            toHashMap(input.Users.Map(user => (user.UserId, user))),
            CreateLockBoxCodesForReservations(options, input.Command.Date, upcomingReservations, input.LockBoxCodes));

    static SendLockBoxCodesOutput SendLockBoxCodesCore(
        Seq<ReservationWithOrder> upcomingReservations, HashMap<UserId, UserExcerpt> users, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes) =>
        new(
            SetReservationsEmailFlag(upcomingReservations, ReservationEmails.LockBoxCode),
            upcomingReservations.Map(reservation => CreateEmail(users, lockBoxCodes, reservation)).ToSeq());

    static LockBoxCodesEmail CreateEmail(
        HashMap<UserId, UserExcerpt> users, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, ReservationWithOrder reservationWithOrder) =>
        CreateEmail(lockBoxCodes, reservationWithOrder, GetUser(users, reservationWithOrder.Order.UserId));

    static UserExcerpt GetUser(HashMap<UserId, UserExcerpt> users, UserId userId) =>
        users.Find(userId).Case switch
        {
            UserExcerpt user => user,
            _ => throw new UnreachableException(),
        };

    static LockBoxCodesEmail CreateEmail(
        HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, ReservationWithOrder reservationWithOrder, UserExcerpt user) =>
        new(
            user.Email(),
            user.FullName,
            reservationWithOrder.Order.OrderId,
            reservationWithOrder.Reservation.ResourceId,
            reservationWithOrder.Reservation.Extent.Date,
            GetLockBoxCodes(lockBoxCodes, reservationWithOrder.Reservation));

    static Seq<DatedLockBoxCode> GetLockBoxCodes(HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, Reservation reservation) =>
        lockBoxCodes.Find(reservation).Case switch
        {
            Seq<DatedLockBoxCode> datedLockBoxCodes => datedLockBoxCodes,
            _ => throw new UnreachableException(),
        };
}
