using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Orders;

static class OrderDetailsFactory
{
    public static OrderDetailsDto CreateOrderDetails(OrderingOptions options, LocalDate today, OrderDetails orderDetails) =>
        !orderDetails.Order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? CreateResidentOrderDetails(options, today, orderDetails)
            : CreateOwnerOrderDetails(options, today, orderDetails);

    static OrderDetailsDto CreateResidentOrderDetails(OrderingOptions options, LocalDate today, OrderDetails orderDetails) =>
        new(
            orderDetails.Order.OrderId,
            OrderType.Resident,
            orderDetails.Order.CreatedTimestamp,
            CreateUserIdentity(orderDetails.User),
            CreateResidentReservations(options, today, orderDetails.Order.Reservations),
            orderDetails.Order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            new(
                orderDetails.User.AccountNumber.ToNullable()?.ToString(),
                orderDetails.Order.Specifics.Resident.NoFeeCancellationIsAllowedBefore.ToNullable(),
                orderDetails.Order.Specifics.Resident.AdditionalLineItems),
            Owner: null,
            orderDetails.Order.Audits.Map(audit => CreateOrderAudit(audit, orderDetails.AuditUserFullNames)));

    static Seq<ReservationDto> CreateResidentReservations(OrderingOptions options, LocalDate today, Seq<Reservation> reservations) =>
        reservations.Map(reservation =>
            new ReservationDto(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price.Case switch
                {
                    Price price => price,
                    _ => throw new UnreachableException(),
                },
                reservation.Extent,
                CanReservationBeCancelled(options, today, reservation.Status, reservation.Extent, alwaysAllowCancellation: true),
                GetEntryCode(options, today, reservation)));

    static OrderDetailsDto CreateOwnerOrderDetails(OrderingOptions options, LocalDate today, OrderDetails orderDetails) =>
        new(
            orderDetails.Order.OrderId,
            OrderType.Owner,
            orderDetails.Order.CreatedTimestamp,
            CreateUserIdentity(orderDetails.User),
            CreateOwnerReservations(options, today, orderDetails.Order.Reservations),
            orderDetails.Order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            Resident: null,
            new(
                orderDetails.Order.Specifics.AsT1.Description,
                orderDetails.Order.Flags.HasFlag(OrderFlags.IsCleaningRequired)),
            orderDetails.Order.Audits.Map(audit => CreateOrderAudit(audit, orderDetails.AuditUserFullNames)));

    public static IEnumerable<ReservationDto> CreateOwnerReservations(OrderingOptions options, LocalDate today, Seq<Reservation> reservations) =>
        reservations.Map(reservation =>
            new ReservationDto(
                reservation.ResourceId,
                reservation.Status,
                Price: null,
                reservation.Extent,
                CanBeCancelled: true,
                GetEntryCode(options, today, reservation)));

    static OrderAuditDto CreateOrderAudit(OrderAudit audit, HashMap<UserId, string> userFullNames) =>
        new(
            audit.Timestamp,
            audit.UserId.ToNullable(),
            audit.UserId.Case switch
            {
                UserId userId => userFullNames[userId],
                _ => null,
            },
            audit.Type,
            audit.TransactionId.ToNullable());

    static EntryCode? GetEntryCode(OrderingOptions options, LocalDate today, Reservation reservation) =>
        reservation.EntryCode is not null &&
        reservation.Extent.Date.Minus(options.RevealEntryCodeBeforeReservationStart) <= today &&
        today <= reservation.Extent.Ends().PlusDays(1)
            ? reservation.EntryCode
            : null;
}
