using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateResidentOrderCommand(
    Instant Timestamp,
    UserId AdministratorId,
    OrderId OrderId,
    Option<string> AccountNumber,
    HashSet<ReservationIndex> CancelledReservations,
    bool WaiveFee,
    bool AllowCancellationWithoutFee);
