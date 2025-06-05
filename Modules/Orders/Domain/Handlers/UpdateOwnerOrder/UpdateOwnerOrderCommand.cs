using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateOwnerOrderCommand(
    Instant Timestamp,
    UserId UserId,
    OrderId OrderId,
    Option<string> Description,
    HashSet<ReservationIndex> CancelledReservations,
    Option<bool> IsCleaningRequired);
