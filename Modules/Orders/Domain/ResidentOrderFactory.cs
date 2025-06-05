using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.PaymentFunctions;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class ResidentOrderFactory
{
    public static Seq<ResidentOrder> CreateResidentOrders(
        OrderingOptions options, LocalDate today, Seq<Order> orders, User user, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes) =>
        orders.Map(order => CreateResidentOrder(options, today, order, user, lockBoxCodes));

    public static ResidentOrder CreateResidentOrder(
        OrderingOptions options, LocalDate today, Order order, User user, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes) =>
        CreateResidentOrder(
            options,
            today,
            order,
            user,
            lockBoxCodes,
            GetResident(order));

    static Resident GetResident(Order order) =>
        order.Specifics.Match(
            resident => resident,
            _ => throw new UnreachableException());

    static ResidentOrder CreateResidentOrder(
        OrderingOptions options, LocalDate today, Order order, User user, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, Resident resident) =>
        CreateResidentOrder(
            options,
            order,
            user,
            CreateReservations(
                options,
                today,
                order,
                lockBoxCodes, GetNoFeeCancellationIsAllowed(order, resident)), resident);

    static bool GetNoFeeCancellationIsAllowed(Order order, Resident resident) =>
        order.UpdatedTimestamp() <= Core.OptionExtensions.ToNullable(resident.NoFeeCancellationIsAllowedBefore);

    static Seq<ResidentReservation> CreateReservations(
        OrderingOptions options, LocalDate today, Order order, HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, bool noFeeCancellationIsAllowed) =>
        order.Reservations.Map(
            reservation => CreateResidentReservation(
                options,
                today,
                noFeeCancellationIsAllowed,
                lockBoxCodes, reservation));

    static ResidentReservation CreateResidentReservation(
        OrderingOptions options,
        LocalDate today,
        bool noFeeCancellationIsAllowed,
        HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes,
        Reservation reservation) =>
        new(
            reservation.ResourceId,
            reservation.Status,
            reservation.Price.Case switch
            {
                Price price => price,
                _ => throw new UnreachableException(),
            },
            reservation.Extent,
            CanReservationBeCancelled(options, today, reservation.Status, reservation.Extent, noFeeCancellationIsAllowed),
            GetLockBoxCodesForReservation(lockBoxCodes, reservation));

    static Seq<DatedLockBoxCode> GetLockBoxCodesForReservation(HashMap<Reservation, Seq<DatedLockBoxCode>> lockBoxCodes, Reservation reservation) =>
        lockBoxCodes.Find(reservation).Case switch
        {
            Seq<DatedLockBoxCode> lockBoxCodesForReservation => lockBoxCodesForReservation,
            _ => Empty,
        };

    static ResidentOrder CreateResidentOrder(OrderingOptions options, Order order, User user, Seq<ResidentReservation> reservations, Resident resident) =>
        new(
            order.OrderId,
            order.CreatedTimestamp,
            reservations,
            order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && reservations.Any(reservation => reservation.CanBeCancelled),
            order.Price(),
            resident.NoFeeCancellationIsAllowedBefore,
            order.NeedsConfirmation() ? GetPaymentInformation(options, user) : None,
            resident.AdditionalLineItems,
            user);
}
