using Frederikskaj2.Reservations.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Users;

public record UserDetailsDto(
        UserIdentityDto Identity,
        bool IsEmailConfirmed,
        Roles Roles,
        bool IsPendingDelete,
        bool IsDeleted,
        IEnumerable<OrderId> Orders,
        IEnumerable<OrderId> HistoryOrders,
        Amount Balance,
        PaymentId PaymentId,
        Instant? LatestSignIn,
        IEnumerable<UserAuditDto> Audits)
    : UserDto(Identity, IsEmailConfirmed, Roles, IsPendingDelete, IsDeleted, Orders, HistoryOrders);
