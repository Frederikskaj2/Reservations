using LanguageExt;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Users;

public static class UserDetailsFactory
{
    public static UserDetailsDto CreateUserDetails(User user, HashMap<UserId, string> auditUserFullNames) =>
        new(
            CreateUserIdentity(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.Flags.HasFlag(UserFlags.IsDeleted),
            user.Orders,
            user.HistoryOrders,
            -user.Balance(),
            FromUserId(user.UserId),
            user.LatestSignIn.ToNullable(),
            user.Audits.Map(audit => CreateUserAudit(audit, auditUserFullNames)));

    static UserAuditDto CreateUserAudit(UserAudit audit, HashMap<UserId, string> auditUserFullNames) =>
        new(
            audit.Timestamp,
            audit.UserId.ToNullable(),
            audit.UserId.Case switch
            {
                UserId userId => auditUserFullNames[userId],
                _ => null,
            },
            audit.Type,
            audit.OrderId.ToNullable(),
            audit.TransactionId.ToNullable());
}
