using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutDetails(
    PayOutId PayOutId,
    Instant CreatedTimestamp,
    UserExcerpt Resident,
    AccountNumber AccountNumber,
    Amount Amount,
    PayOutStatus Status,
    Option<PayOutResolution> Resolution,
    Seq<PayOutNote> Notes,
    Seq<PayOutAudit> Audits,
    HashMap<UserId, string> UserFullNames,
    Option<int> DelayedDays);
