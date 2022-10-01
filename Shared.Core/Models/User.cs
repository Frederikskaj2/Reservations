using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record User(
    UserInformation Information,
    bool IsEmailConfirmed,
    Roles Roles,
    bool IsPendingDelete,
    bool IsDeleted,
    IEnumerable<OrderId> Orders,
    IEnumerable<OrderId> HistoryOrders);
