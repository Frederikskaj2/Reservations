using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record OrderDto(
    OrderId OrderId,
    OrderType Type,
    Instant CreatedTimestamp,
    UserIdentityDto UserIdentity,
    IEnumerable<ReservationDto> Reservations,
    bool IsHistoryOrder,
    ResidentOrderDetailsDto? Resident,
    OwnerOrderDetailsDto? Owner);
