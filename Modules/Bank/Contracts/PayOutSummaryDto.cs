using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutSummaryDto(
    PayOutId PayOutId,
    Instant CreatedTimestamp,
    UserIdentityDto UserIdentity,
    PaymentId PaymentId,
    string AccountNumber,
    Amount Amount,
    PayOutStatus Status,
    Instant? ResolvedTimestamp,
    int? DelayedDays);
