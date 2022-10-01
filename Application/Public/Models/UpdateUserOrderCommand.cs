using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateUserOrderCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    OrderId OrderId,
    Option<string> AccountNumber,
    HashSet<ReservationIndex> CancelledReservations,
    bool WaiveFee,
    bool AllowCancellationWithoutFee);
