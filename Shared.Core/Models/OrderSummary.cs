using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record OrderSummary(
    OrderId OrderId,
    OrderType Type,
    Instant CreatedTimestamp,
    UserInformation UserInformation);
