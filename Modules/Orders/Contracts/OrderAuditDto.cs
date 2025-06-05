using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record OrderAuditDto(Instant Timestamp, UserId? UserId, string? FullName, OrderAuditType Type, TransactionId? TransactionId);
