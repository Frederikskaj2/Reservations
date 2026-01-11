using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Bank;

static class PayOutFactory
{
    public static IEnumerable<PayOutSummaryDto> CreatePayOutSummaries(Seq<PayOutSummary> summaries) =>
        summaries.Map(CreatePayOutSummary);

    public static PayOutSummaryDto CreatePayOutSummary(PayOutSummary summary) =>
        new(
            summary.PayOutId,
            summary.CreatedTimestamp,
            CreateUserIdentity(summary.Resident),
            FromUserId(summary.Resident.UserId),
            summary.AccountNumber.ToString(),
            summary.Amount,
            summary.Status,
            GetResolvedTimestamp(summary.Resolution),
            summary.DelayedDays.ToNullable());

    public static PayOutDetailsDto CreatePayOutDetails(PayOutDetails details) =>
        new(
            details.PayOutId,
            details.CreatedTimestamp,
            CreateUserIdentity(details.Resident),
            FromUserId(details.Resident.UserId),
            details.AccountNumber.ToString(),
            details.Amount,
            details.Status,
            GetResolvedTimestamp(details.Resolution),
            details.Notes.Map(note => CreateNote(note, details.UserFullNames)),
            details.Audits.Map(audit => CreateAudit(audit, details.UserFullNames)),
            details.DelayedDays.ToNullable());

    static Instant? GetResolvedTimestamp(Option<PayOutResolution> resolutionOption) =>
        resolutionOption.Case switch
        {
            PayOutResolution resolution => resolution.Match(reconciled => reconciled.Timestamp, cancelled => cancelled.Timestamp),
            _ => null,
        };

    static PayOutNoteDto CreateNote(PayOutNote note, HashMap<UserId, string> userFullNames) =>
        new(
            note.Timestamp,
            note.UserId,
            userFullNames[note.UserId],
            note.Text);

    static PayOutAuditDto CreateAudit(PayOutAudit audit, HashMap<UserId, string> userFullNames) =>
        new(
            audit.Timestamp,
            audit.UserId,
            userFullNames[audit.UserId],
            audit.Type);
}
