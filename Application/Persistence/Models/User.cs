using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

record User(UserId UserId, Seq<EmailStatus> Emails, string FullName, string Phone, ApartmentId? ApartmentId, UserSecurity Security, Roles Roles)
{
    public UserFlags Flags { get; init; }
    public Instant? LatestSignIn { get; init; }
    public Instant? LatestDebtReminder { get; init; }
    public string? AccountNumber { get; init; }
    public EmailSubscriptions EmailSubscriptions { get; init; }
    public FailedSignIn? FailedSign { get; init; }
    public AccountAmounts Accounts { get; init; } = AccountAmounts.Empty;
    public Seq<OrderId> Orders { get; init; } = Empty;
    public Seq<OrderId> HistoryOrders { get; init; } = Empty;
    public Seq<UserAudit> Audits { get; init; }

    public EmailAddress Email() => !Flags.HasFlag(UserFlags.IsDeleted) ? Emails.Head.Email : EmailAddress.Deleted;

    public bool IsEmailConfirmed() => Flags.HasFlag(UserFlags.IsDeleted) || Emails.Head.IsConfirmed;

    public Instant Created() => Audits.Head.Timestamp;

    // Debit (positive): The user owes money. Credit (negative): We owe the user money.
    public Amount Balance() => Accounts[Account.AccountsReceivable] + Accounts[Account.AccountsPayable];

    public static string GetId(UserId userId) => userId.ToString();
}
