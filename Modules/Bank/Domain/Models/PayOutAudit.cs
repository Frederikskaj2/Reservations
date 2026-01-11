using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutAudit(Instant Timestamp, UserId UserId, PayOutAuditType Type);
