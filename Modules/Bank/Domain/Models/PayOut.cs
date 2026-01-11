using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public sealed record PayOut(
    PayOutId PayOutId,
    Instant CreatedTimestamp,
    UserId ResidentId,
    AccountNumber AccountNumber,
    Amount Amount,
    PayOutStatus Status,
    Option<PayOutResolution> Resolution,
    Seq<PayOutNote> Notes,
    Seq<PayOutAudit> Audits)
    : IHasId
{
    string IHasId.GetId() => PayOutId.GetId();
}
