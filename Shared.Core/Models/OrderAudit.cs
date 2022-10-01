using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record OrderAudit(Instant Timestamp, UserId? UserId, string? FullName, OrderAuditType Type, TransactionId? TransactionId);
