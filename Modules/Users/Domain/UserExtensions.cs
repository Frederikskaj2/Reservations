using Frederikskaj2.Reservations.Core;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class UserExtensions
{
    public static User UpdateApartmentId(this User user, Instant timestamp, ApartmentId apartmentId, UserId updatedByUserId) =>
        user.ApartmentId != apartmentId
            ? user with
            {
                ApartmentId = apartmentId,
                Audits = user.Audits.Add(UserAudit.UpdateApartmentId(timestamp, updatedByUserId)),
            }
            : user;

    public static User SetAccountNumber(this User user, Instant timestamp, string accountNumber, UserId updatedByUserId) =>
        user.AccountNumber != accountNumber
            ? user with
            {
                AccountNumber = accountNumber,
                Audits = user.Audits.Add(UserAudit.SetAccountNumber(timestamp, updatedByUserId)),
            }
            : user;

    public static User RemoveAccountNumber(this User user, Instant timestamp, UserId updatedByUserId) =>
        user.AccountNumber.IsSome
            ? user with
            {
                AccountNumber = None,
                Audits = user.Audits.Add(UserAudit.RemoveAccountNumber(timestamp, updatedByUserId)),
            }
            : user;

    public static User UpdateFullName(this User user, Instant timestamp, string fullName, UserId updatedByUserId) =>
        user.FullName != fullName
            ? user with
            {
                FullName = fullName,
                Audits = user.Audits.Add(UserAudit.UpdateFullName(timestamp, updatedByUserId)),
            }
            : user;

    public static User UpdatePhone(this User user, Instant timestamp, string phone, UserId updatedByUserId) =>
        user.Phone != phone
            ? user with
            {
                Phone = phone,
                Audits = user.Audits.Add(UserAudit.UpdatePhone(timestamp, updatedByUserId)),
            }
            : user;

    public static User UpdateEmailSubscriptions(this User user, Instant timestamp, EmailSubscriptions subscriptions) =>
        user.EmailSubscriptions != subscriptions
            ? user with
            {
                EmailSubscriptions = subscriptions,
                Audits = user.Audits.Add(UserAudit.UpdateEmailSubscriptions(timestamp, user.UserId)),
            }
            : user;

    public static User UpdateRoles(this User user, Instant timestamp, Roles roles, UserId updatedByUserId) =>
        user.Roles != roles
            ? user with
            {
                Roles = roles,
                Audits = user.Audits.Add(UserAudit.UpdateRoles(timestamp, updatedByUserId)),
            }
            : user;

    public static User UpdateIsPendingDelete(this User user, Instant timestamp, bool isPendingDelete, UserId updatedByUserId) =>
        isPendingDelete && !user.Flags.HasFlag(UserFlags.IsPendingDelete)
            ? user with
            {
                Flags = user.Flags | UserFlags.IsPendingDelete,
                Audits = user.Audits.Add(UserAudit.RequestDelete(timestamp, updatedByUserId)),
            }
            : user;
}
