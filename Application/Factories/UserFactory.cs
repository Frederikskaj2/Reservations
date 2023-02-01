using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared.Core;
using static Frederikskaj2.Reservations.Application.PaymentIdEncoder;

namespace Frederikskaj2.Reservations.Application;

static class UserFactory
{
    public static MyUser CreateMyUser(User user) =>
        new(
            CreateUserInformation(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.EmailSubscriptions,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.AccountNumber);

    public static IEnumerable<Shared.Core.User> CreateUsers(IEnumerable<User> users) =>
        users.Map(CreateUser);

    static Shared.Core.User CreateUser(User user) =>
        new(
            CreateUserInformation(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.Flags.HasFlag(UserFlags.IsDeleted),
            user.Orders,
            user.HistoryOrders);

    public static UserDetails CreateUserDetails(User user, LanguageExt.HashMap<UserId, string> userFullNames) =>
        new(
            CreateUserInformation(user),
            user.IsEmailConfirmed(),
            user.Roles,
            user.Flags.HasFlag(UserFlags.IsPendingDelete),
            user.Flags.HasFlag(UserFlags.IsDeleted),
            user.Orders,
            user.HistoryOrders,
            -user.Balance(),
            FromUserId(user.UserId),
            user.LatestSignIn,
            user.Audits.Map(audit => CreateUserAudit(audit, userFullNames)));

    static Shared.Core.UserAudit CreateUserAudit(UserAudit audit, LanguageExt.HashMap<UserId, string> userFullNames) =>
        new(audit.Timestamp, audit.UserId, audit.UserId.HasValue ? userFullNames[audit.UserId.Value] : null, audit.Type, audit.OrderId, audit.TransactionId);

    public static UserInformation CreateUserInformation(User user) =>
        new(user.UserId, user.Email(), user.FullName, user.Phone, user.ApartmentId);
}
