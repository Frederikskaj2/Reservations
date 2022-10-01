using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record Order(
    OrderId OrderId,
    OrderType Type,
    Instant CreatedTimestamp,
    UserInformation UserInformation,
    IEnumerable<Reservation> Reservations,
    bool IsHistoryOrder,
    UserOrder? User,
    OwnerOrder? Owner);
