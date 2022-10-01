using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateMyOrderCommand(
    Instant Timestamp, UserId UserId, OrderId OrderId, Option<string> AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee);
