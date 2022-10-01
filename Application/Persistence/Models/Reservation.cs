using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

record Reservation(
    ResourceId ResourceId,
    ReservationStatus Status,
    Extent Extent,
    Price? Price,
    ReservationEmails SentEmails,
    CleaningPeriod? Cleaning)
{
    public override string ToString() => $"{ResourceId} {Extent}";
}
