using Frederikskaj2.Reservations.Users;
using NodaTime;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Orders;

static class RemoveAccountNumbers
{
    public static RemoveAccountNumbersOutput RemoveAccountNumbersCore(OrderingOptions options, RemoveAccountNumbersInput input) =>
        new(input.Users.Map(user => TryRemoveAccountNumber(options.RemoveAccountNumberAfter, input.Command.Timestamp, input.Command.AdministratorId, user)));

    static User TryRemoveAccountNumber(Duration removeAccountNumbersAfter, Instant timestamp, UserId administratorId, User user) =>
        user.Orders.IsEmpty && user.Balance() == Amount.Zero && GetLatestPayOut(user) + removeAccountNumbersAfter < timestamp
            ? user.RemoveAccountNumber(timestamp, administratorId)
            : user;

    static Instant GetLatestPayOut(User user) =>
        user.Audits.Filter(audit => audit.Type is UserAuditType.PayOut).Last().Timestamp;
}
