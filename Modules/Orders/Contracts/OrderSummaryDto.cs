using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record OrderSummaryDto(
    OrderId OrderId,
    OrderType Type,
    Instant CreatedTimestamp,
    LocalDate NextReservationDate,
    OrderCategory Category,
    string? Description,
    UserIdentityDto User);
