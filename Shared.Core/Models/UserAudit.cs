using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record UserAudit(Instant Timestamp, UserId? UserId, string? FullName, UserAuditType Type, OrderId? OrderId, TransactionId? TransactionId);
