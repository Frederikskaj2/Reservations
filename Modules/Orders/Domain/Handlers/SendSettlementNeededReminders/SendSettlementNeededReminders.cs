using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;

namespace Frederikskaj2.Reservations.Orders;

static class SendSettlementNeededReminders
{
    public static SendSettlementNeededRemindersOutput SendSettlementNeededRemindersCore(OrderingOptions options, SendSettlementNeededRemindersInput input) =>
        SendSettlementNeededRemindersCore(GetReservationsToSettle(input.Orders, input.Command.Date.PlusDays(-options.AdditionalDaysWhereCleaningCanBeDone)));

    static Seq<ReservationWithOrder> GetReservationsToSettle(Seq<Order> orders, LocalDate endBefore) =>
        orders.Bind(
                order => order.Reservations
                    .Map((index, reservation) => new ReservationWithOrder(reservation, order, index))
                    .Filter(
                        reservationWithOrder =>
                            reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed &&
                            !reservationWithOrder.Reservation.SentEmails.HasFlag(ReservationEmails.NeedsSettlement) &&
                            reservationWithOrder.Reservation.Extent.Ends() < endBefore))
            .ToSeq();

    static SendSettlementNeededRemindersOutput SendSettlementNeededRemindersCore( Seq<ReservationWithOrder> reservationsToSettle) =>
        new(SetReservationsEmailFlag(reservationsToSettle, ReservationEmails.NeedsSettlement), reservationsToSettle);
}
