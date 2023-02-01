using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record UserDetails(
        UserInformation Information, bool IsEmailConfirmed, Roles Roles, bool IsPendingDelete, bool IsDeleted,
        IEnumerable<OrderId> Orders, IEnumerable<OrderId> HistoryOrders, Amount Balance, PaymentId PaymentId, Instant? LatestSignIn,
        IEnumerable<UserAudit> Audits)
    : User(Information, IsEmailConfirmed, Roles, IsPendingDelete, IsDeleted, Orders, HistoryOrders);
