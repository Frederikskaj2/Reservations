using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record OrderSummary(
    OrderId OrderId,
    OrderFlags Flags,
    Instant CreatedTimestamp,
    LocalDate NextReservationDate,
    OrderCategory Category,
    Option<string> Description,
    UserExcerpt User);
