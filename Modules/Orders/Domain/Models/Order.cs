using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public sealed record Order(
    OrderId OrderId,
    UserId UserId,
    OrderFlags Flags,
    Instant CreatedTimestamp,
    OrderSpecifics Specifics,
    Seq<Reservation> Reservations,
    Seq<OrderAudit> Audits)
    : IHasId
{
    public Price Price() => Reservations.Fold(
        new Price(),
        (accumulator, reservation) => reservation.Price.Case switch
        {
            Price price => accumulator + price,
            _ => accumulator,
        });

    public bool IsResidentOrder() => !IsOwnerOrder();

    public bool IsOwnerOrder() => Flags.HasFlag(OrderFlags.IsOwnerOrder);

    public bool IsHistoryOrder() => Flags.HasFlag(OrderFlags.IsHistoryOrder);

    public Instant UpdatedTimestamp() => Audits.Last.Timestamp;

    public bool NeedsConfirmation() => Reservations.Any(reservation => reservation.Status is ReservationStatus.Reserved);

    public bool IsConfirmed() => Reservations.All(reservation => reservation.Status is ReservationStatus.Confirmed);

    public Reservation this[ReservationIndex index] => Reservations[index.ToInt32()];

    string IHasId.GetId() => OrderId.GetId();
}
