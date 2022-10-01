using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateOwnerOrderCommand(
    Instant Timestamp,
    UserId UserId,
    OrderId OrderId,
    Option<string> Description,
    HashSet<ReservationIndex> CancelledReservations,
    Option<bool> IsCleaningRequired);
