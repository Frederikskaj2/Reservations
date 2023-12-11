using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

static class UserAuditFunctions
{
    public static User UpdateApartmentId(this User user, Instant timestamp, ApartmentId apartmentId) =>
        user.ApartmentId != apartmentId
            ? user with
            {
                ApartmentId = apartmentId,
                Audits = user.Audits.Add(new(timestamp, user.UserId, UserAuditType.UpdateApartmentId))
            }
            : user;

    public static User SetAccountNumber(this User user, Instant timestamp, string accountNumber, UserId userId) =>
        user.AccountNumber != accountNumber
            ? user with
            {
                AccountNumber = accountNumber,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.SetAccountNumber))
            }
            : user;

    public static User RemoveAccountNumber(this User user, Instant timestamp, UserId userId) =>
        user.AccountNumber is not null
            ? user with
            {
                AccountNumber = null,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.RemoveAccountNumber))
            }
            : user;

    public static User UpdateFullName(this User user, Instant timestamp, string fullName, UserId userId) =>
        user.FullName != fullName
            ? user with
            {
                FullName = fullName,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.UpdateFullName))
            }
            : user;

    public static User UpdatePhone(this User user, Instant timestamp, string phone, UserId userId) =>
        user.Phone != phone
            ? user with
            {
                Phone = phone,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.UpdatePhone))
            }
            : user;

    public static User UpdateEmailSubscriptions(this User user, Instant timestamp, EmailSubscriptions subscriptions) =>
        user.EmailSubscriptions != subscriptions
            ? user with
            {
                EmailSubscriptions = subscriptions,
                Audits = user.Audits.Add(new(timestamp, user.UserId, UserAuditType.UpdateEmailSubscriptions))
            }
            : user;

    public static User UpdateRoles(this User user, Instant timestamp, Roles roles, UserId userId) =>
        user.Roles != roles
            ? user with
            {
                Roles = roles,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.UpdateRoles))
            }
            : user;

    public static User UpdateIsPendingDelete(this User user, Instant timestamp, bool isPendingDelete, UserId userId) =>
        isPendingDelete && !user.Flags.HasFlag(UserFlags.IsPendingDelete)
            ? user with
            {
                Flags = user.Flags | UserFlags.IsPendingDelete,
                Audits = user.Audits.Add(new(timestamp, userId, UserAuditType.RequestDelete))
            }
            : user;
}
