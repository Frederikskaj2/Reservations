using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record UserAuditDto(Instant Timestamp, UserId? UserId, string? FullName, UserAuditType Type, OrderId? OrderId, TransactionId? TransactionId);
