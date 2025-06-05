using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.LockBox;

public record LockBoxCodesOverviewEmail(EmailAddress Email, string FullName, Seq<WeeklyLockBoxCodes> Codes);
