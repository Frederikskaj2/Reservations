using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public record OrderAudit(Instant Timestamp, Option<UserId> UserId, OrderAuditType Type, Option<TransactionId> TransactionId)
{
    public static OrderAudit PlaceResidentOrder(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, OrderAuditType.PlaceOrder, transactionId);

    public static OrderAudit PlaceOwnerOrder(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.PlaceOrder, None);

    public static OrderAudit ConfirmOrder(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, OrderAuditType.ConfirmOrder, transactionId);

    public static OrderAudit SettleReservation(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, OrderAuditType.SettleReservation, transactionId);

    public static OrderAudit CancelResidentReservation(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, OrderAuditType.CancelReservation, transactionId);

    public static OrderAudit CancelOwnerReservation(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.CancelReservation, None);

    public static OrderAudit AllowCancellationWithoutFee(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.AllowCancellationWithoutFee, None);

    public static OrderAudit DisallowCancellationWithoutFee(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.DisallowCancellationWithoutFee, None);

    public static OrderAudit UpdateDescription(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.UpdateDescription, None);

    public static OrderAudit UpdateCleaning(Instant timestamp, UserId userId) =>
        new(timestamp, userId, OrderAuditType.UpdateCleaning, None);

    public static OrderAudit FinishOrder(Instant timestamp) =>
        new(timestamp, None, OrderAuditType.FinishOrder, None);

    public static OrderAudit UpdateReservations(Instant timestamp, UserId userId, TransactionId transactionId) =>
        new(timestamp, userId, OrderAuditType.UpdateReservations, transactionId);
}
