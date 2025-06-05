using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record OrderDetailsDto(
        OrderId OrderId,
        OrderType Type,
        Instant CreatedTimestamp,
        UserIdentityDto UserIdentity,
        IEnumerable<ReservationDto> Reservations,
        bool IsHistoryOrder,
        ResidentOrderDetailsDto? Resident,
        OwnerOrderDetailsDto? Owner,
        IEnumerable<OrderAuditDto> Audits)
    : OrderDto(OrderId, Type, CreatedTimestamp, UserIdentity, Reservations, IsHistoryOrder, Resident, Owner);
