using Frederikskaj2.Reservations.LockBox;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record Reservation(
    ResourceId ResourceId,
    ReservationStatus Status,
    Extent Extent,
    Option<Price> Price,
    ReservationEmails SentEmails)
{
    public override string ToString() => $"{ResourceId} {Extent}";
}
