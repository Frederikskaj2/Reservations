using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record OrderExcerpt(
    OrderId OrderId,
    UserId UserId,
    OrderFlags Flags,
    Instant CreatedTimestamp,
    Option<string> Description,
    Seq<Reservation> Reservations);
