using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutSummary(
    PayOutId PayOutId,
    Instant CreatedTimestamp,
    UserExcerpt Resident,
    AccountNumber AccountNumber,
    Amount Amount,
    PayOutStatus Status,
    Option<PayOutResolution> Resolution,
    Option<int> DelayedDays);
