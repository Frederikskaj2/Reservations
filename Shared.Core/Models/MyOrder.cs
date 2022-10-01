using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record MyOrder(
    OrderId OrderId,
    Instant CreatedTimestamp,
    IEnumerable<Reservation>? Reservations,
    bool IsHistoryOrder,
    bool CanBeEdited,
    Price Price,
    Instant? NoFeeCancellationIsAllowedBefore,
    PaymentInformation? Payment,
    IEnumerable<LineItem> AdditionalLineItems,
    UserInformation UserInformation);
