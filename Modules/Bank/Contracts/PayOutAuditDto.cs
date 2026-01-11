using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutAuditDto(Instant Timestamp, UserId UserId, string FullName, PayOutAuditType Type);
