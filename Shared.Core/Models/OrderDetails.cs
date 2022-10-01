using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record OrderDetails(
        OrderId OrderId, OrderType Type, Instant CreatedTimestamp, UserInformation UserInformation, IEnumerable<Reservation> Reservations, bool IsHistoryOrder,
        UserOrder? User, OwnerOrder? Owner, IEnumerable<OrderAudit> Audits)
    : Order(OrderId, Type, CreatedTimestamp, UserInformation, Reservations, IsHistoryOrder, User, Owner);
