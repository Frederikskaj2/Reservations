using Frederikskaj2.Reservations.LockBox;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record ResidentReservation(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price Price,
    Extent Extent,
    bool CanBeCancelled,
    Seq<DatedLockBoxCode> LockBoxCodes);
