using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutDetailsDto(
    PayOutId PayOutId,
    Instant CreatedTimestamp,
    UserIdentityDto UserIdentity,
    PaymentId PaymentId,
    string AccountNumber,
    Amount Amount,
    PayOutStatus Status,
    Instant? ResolvedTimestamp,
    IEnumerable<PayOutNoteDto> Notes,
    IEnumerable<PayOutAuditDto> Audits,
    int? DelayedDays);
