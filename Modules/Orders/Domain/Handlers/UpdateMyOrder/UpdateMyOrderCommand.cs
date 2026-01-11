using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateMyOrderCommand(
    Instant Timestamp,
    UserId UserId,
    OrderId OrderId,
    Option<AccountNumber> AccountNumber,
    HashSet<ReservationIndex> CancelledReservations);
