using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record Order(
    OrderId OrderId,
    UserId UserId,
    OrderFlags Flags,
    Instant CreatedTimestamp,
    UserOrder? User,
    OwnerOrder? Owner,
    Seq<Reservation> Reservations,
    Seq<OrderAudit> Audits)
{
    public Price Price() => Reservations.Fold(
        new Price(), (accumulator, reservation) => reservation.Price is not null ? accumulator + reservation.Price : accumulator);

    public bool IsUserOrder() => !IsOwnerOrder();

    public bool IsOwnerOrder() => Flags.HasFlag(OrderFlags.IsOwnerOrder);

    public bool IsHistoryOrder() => Flags.HasFlag(OrderFlags.IsHistoryOrder);

    public Instant UpdatedTimestamp() => Audits.Last.Timestamp;

    public bool NeedsConfirmation() => Reservations.Any(reservation => reservation.Status is ReservationStatus.Reserved);

    public bool IsConfirmed() => Reservations.All(reservation => reservation.Status is ReservationStatus.Confirmed);

    public static string GetId(OrderId orderId) => orderId.ToString();
}
