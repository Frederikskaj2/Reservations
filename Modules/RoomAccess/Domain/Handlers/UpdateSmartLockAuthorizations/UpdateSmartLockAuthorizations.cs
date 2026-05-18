using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.RoomAccess;

static class UpdateSmartLockAuthorizations
{
    public static UpdateSmartLockAuthorizationsOutput UpdateSmartLockAuthorizationsCore(
        OrderingOptions options, ITimeConverter timeConverter, UpdateSmartLockAuthorizationsInput input) =>
        new(input.SmartLockAuthorizationContext.AddAuthorizations(GetActiveSmartLockAuthorizations(options, timeConverter, input)));

    static IEnumerable<SmartLockAuthorization> GetActiveSmartLockAuthorizations(
        OrderingOptions options, ITimeConverter timeConverter, UpdateSmartLockAuthorizationsInput input) =>
        input.Reservations
            .Filter(reservation => HasActiveEntryCode(options, input.Command.Date, reservation))
            .Map(reservationInformation => CreateSmartLockAuthorization(options, timeConverter, reservationInformation));

    static bool HasActiveEntryCode(OrderingOptions options, LocalDate today, ReservationInformation reservation) =>
        reservation.Extent.Date.Minus(options.RevealEntryCodeBeforeReservationStart) <= today &&
        today <= reservation.Extent.Ends().PlusDays(1);

    static SmartLockAuthorization CreateSmartLockAuthorization(
        OrderingOptions options, ITimeConverter timeConverter, ReservationInformation reservation) =>
        new(
            reservation.ResourceId,
            reservation.OrderId,
            timeConverter.GetInstant(reservation.Extent.Date.At(options.CheckInTime)),
            timeConverter.GetInstant(reservation.Extent.Ends().At(options.CheckOutTime).Plus(options.ExtendSmartLockEntryCodePeriod)),
            reservation.EntryCode);
}
