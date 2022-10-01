using System.Diagnostics.CodeAnalysis;
using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/4462")]
public record OrderAudit(Instant Timestamp, UserId? UserId, OrderAuditType Type, TransactionId? TransactionId = null);
