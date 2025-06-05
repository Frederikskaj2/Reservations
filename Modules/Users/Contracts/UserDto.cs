using Frederikskaj2.Reservations.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Users;

public record UserDto(
    UserIdentityDto Identity,
    bool IsEmailConfirmed,
    Roles Roles,
    bool IsPendingDelete,
    bool IsDeleted,
    IEnumerable<OrderId> Orders,
    IEnumerable<OrderId> HistoryOrders);
