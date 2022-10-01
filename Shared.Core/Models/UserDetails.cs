using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record UserDetails : User
{
    public UserDetails(
        UserInformation information, bool isEmailConfirmed, Roles roles, bool isPendingDelete, bool isDeleted,
        IEnumerable<OrderId> orders, IEnumerable<OrderId> historyOrders, Amount balance, Instant? latestSignIn, IEnumerable<UserAudit> audits)
        : base(information, isEmailConfirmed, roles, isPendingDelete, isDeleted, orders, historyOrders) =>
        (Balance, LatestSignIn, Audits) = (balance, latestSignIn, audits);

    public Amount Balance { get; }

    public Instant? LatestSignIn { get; }

    public IEnumerable<UserAudit> Audits { get; }
}
