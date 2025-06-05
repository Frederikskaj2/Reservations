using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class OrdersLockBoxCodesFunctions
{
    public static HashMap<Reservation, Seq<DatedLockBoxCode>> CreateLockBoxCodesForOrders(
        OrderingOptions options, LocalDate today, Seq<Order> orders, LockBoxCodes lockBoxCodes) =>
        toHashMap(CreateLockBoxCodesForReservations(options, today, orders.Bind(order => order.Reservations), CreateLockBoxCodeMap(lockBoxCodes)));

    public static HashMap<Reservation, Seq<DatedLockBoxCode>> CreateLockBoxCodesForOrder(
        OrderingOptions options, LocalDate today, Order order, LockBoxCodes lockBoxCodes) =>
        toHashMap(CreateLockBoxCodesForReservations(options, today, order.Reservations, CreateLockBoxCodeMap(lockBoxCodes)));

    public static HashMap<Reservation, Seq<DatedLockBoxCode>> CreateLockBoxCodesForReservations(
        OrderingOptions options, LocalDate today, Seq<ReservationWithOrder> reservations, LockBoxCodes lockBoxCodes) =>
        toHashMap(CreateLockBoxCodesForReservations(options, today, reservations.Map(reservation => reservation.Reservation), CreateLockBoxCodeMap(lockBoxCodes)));

    static HashMap<(ResourceId, LocalDate), LockBoxCode> CreateLockBoxCodeMap(LockBoxCodes lockBoxCodes) =>
        toHashMap(lockBoxCodes.Codes.Map(code => ((code.ResourceId, code.Date), code)));

    static IEnumerable<(Reservation, Seq<DatedLockBoxCode>)> CreateLockBoxCodesForReservations(
        OrderingOptions options, LocalDate today, IEnumerable<Reservation> reservations, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes) =>
        reservations
            .Map(reservation => (Reservation: reservation, LockBoxCodes: CreateLockBoxCodesForReservation(options, today, reservation, lockBoxCodes)))
            .Filter(tuple => tuple.LockBoxCodes.Any());

    static Seq<DatedLockBoxCode> CreateLockBoxCodesForReservation(
        OrderingOptions options, LocalDate today, Reservation reservation, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes) =>
        reservation.Status is ReservationStatus.Confirmed && reservation.Extent.Date.Plus(-options.RevealLockBoxCodeBeforeReservationStart) <= today
            ? CreateLockBoxCodesForReservation(lockBoxCodes, reservation)
            : Empty;

    static Seq<DatedLockBoxCode> CreateLockBoxCodesForReservation(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Reservation reservation) =>
        CreateLockBoxCodesForReservation(lockBoxCodes, reservation, reservation.Extent.Date.PreviousMonday(), reservation.Extent.Ends().PreviousMonday());

    static Seq<DatedLockBoxCode> CreateLockBoxCodesForReservation(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Reservation reservation, LocalDate firstMonday, LocalDate lastMonday) =>
        somes(
            IntegersFrom(0)
                .Map(firstMonday.PlusWeeks)
                .TakeWhile(monday => monday <= lastMonday)
                .Map(monday => GetDatedLockBoxCode(lockBoxCodes, reservation.ResourceId, monday, GetLockBoxCodeDateForReservation(reservation, monday))))
            .ToSeq();

    static LocalDate GetLockBoxCodeDateForReservation(Reservation reservation, LocalDate monday) =>
        monday < reservation.Extent.Date ? reservation.Extent.Date : monday;

    static Option<DatedLockBoxCode> GetDatedLockBoxCode(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, ResourceId resourceId, LocalDate monday, LocalDate date) =>
        lockBoxCodes.Find((resourceId, monday)).Case switch
        {
            LockBoxCode code => Some(new DatedLockBoxCode(date, code.Code)),
            _ => None,
        };

    [SuppressMessage("Sonar", "S2190:Loops and recursions should not be infinite", Justification = "The caller will break the loop.")]
    static IEnumerable<int> IntegersFrom(int from)
    {
        while (true)
            yield return from++;
        // ReSharper disable once IteratorNeverReturns
    }
}
