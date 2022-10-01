using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Application;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/4462")]
public record UserAudit(Instant Timestamp, UserId? UserId, UserAuditType Type, OrderId? OrderId = null, TransactionId? TransactionId = null)
{
    public static UserAudit Create(Instant timestamp, UserId? userId, UserAuditType type, OrderId orderId) =>
        new(timestamp, userId, type, orderId);

    public static UserAudit Create(Instant timestamp, UserId? userId, UserAuditType type, TransactionId transactionId) =>
        new(timestamp, userId, type, null, transactionId);
}
