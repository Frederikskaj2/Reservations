using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record MyOrderDto(
    OrderId OrderId,
    Instant CreatedTimestamp,
    IEnumerable<MyReservationDto> Reservations,
    bool IsHistoryOrder,
    bool CanBeEdited,
    Price Price,
    Instant? NoFeeCancellationIsAllowedBefore,
    PaymentInformation? Payment,
    IEnumerable<LineItem> AdditionalLineItems,
    UserIdentityDto UserIdentity);
