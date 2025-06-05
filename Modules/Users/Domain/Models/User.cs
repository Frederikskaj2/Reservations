using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public record User(
    UserId UserId,
    Seq<EmailStatus> Emails,
    string FullName,
    string Phone,
    Option<ApartmentId> ApartmentId,
    UserSecurity Security,
    Roles Roles)
    : IHasId
{
    public UserFlags Flags { get; init; }
    public Option<Instant> LatestSignIn { get; init; }
    public Option<Instant> LatestDebtReminder { get; init; }
    public Option<string> AccountNumber { get; init; }
    public EmailSubscriptions EmailSubscriptions { get; init; }
    public Option<FailedSignIn> FailedSign { get; init; }
    public AccountAmounts Accounts { get; init; } = AccountAmounts.Empty;
    public Seq<OrderId> Orders { get; init; } = Empty;
    public Seq<OrderId> HistoryOrders { get; init; } = Empty;
    public Seq<UserAudit> Audits { get; init; }

    public EmailAddress Email() => !Flags.HasFlag(UserFlags.IsDeleted) ? Emails.Head.Email : EmailAddress.Deleted;

    public bool IsEmailConfirmed() => Flags.HasFlag(UserFlags.IsDeleted) || Emails.Head.IsConfirmed;

    public Instant Created() => Audits.Head.Timestamp;

    // Debit (positive): The resident owes money. Credit (negative): We owe the resident money.
    public Amount Balance() => Accounts[Account.AccountsReceivable] + Accounts[Account.AccountsPayable];

    public bool HasDebt() => Balance() > Amount.Zero;

    public bool IsOwedMoney() => Balance() < Amount.Zero;

    public static string GetId(UserId userId) => userId.ToString()!;

    string IHasId.GetId() => UserId.GetId();
}
