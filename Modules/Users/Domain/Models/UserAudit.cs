using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public record UserAudit(Instant Timestamp, Option<UserId> UserId, UserAuditType Type, Option<OrderId> OrderId, Option<TransactionId> TransactionId)
{
    public static UserAudit SignUp(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.SignUp, None, None);

    public static UserAudit ConfirmEmail(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.ConfirmEmail, None, None);

    public static UserAudit RequestResendConfirmEmail(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.RequestResendConfirmEmail, None, None);

    public static UserAudit RequestNewPassword(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.RequestNewPassword, None, None);

    public static UserAudit UpdatePassword(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdatePassword, None, None);

    public static UserAudit UpdateApartmentId(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdateApartmentId, None, None);

    public static UserAudit UpdateFullName(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdateFullName, None, None);

    public static UserAudit UpdatePhone(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdatePhone, None, None);

    public static UserAudit SetAccountNumber(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.SetAccountNumber, None, None);

    public static UserAudit RemoveAccountNumber(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.RemoveAccountNumber, None, None);

    public static UserAudit UpdateEmailSubscriptions(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdateEmailSubscriptions, None, None);

    public static UserAudit UpdateRoles(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.UpdateRoles, None, None);

    public static UserAudit CreateOrder(Instant timestamp, UserId userId, OrderId orderId) =>
        new(timestamp, userId, UserAuditType.PlaceOrder, orderId, None);

    public static UserAudit CreateOwnerOrder(Instant timestamp, UserId userId, OrderId orderId) =>
        new(timestamp, userId, UserAuditType.PlaceOwnerOrder, orderId, None);

    public static UserAudit PayIn(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, UserAuditType.PayIn, None, transactionId);

    public static UserAudit PayOut(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, UserAuditType.PayOut, None, transactionId);

    public static UserAudit RequestDelete(Instant timestamp, UserId userId) =>
        new(timestamp, userId, UserAuditType.RequestDelete, None, None);

    public static UserAudit Delete(Instant timestamp) =>
        new(timestamp, None, UserAuditType.Delete, None, None);

    public static UserAudit Reimburse(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, UserAuditType.Reimburse, None, transactionId);
}
