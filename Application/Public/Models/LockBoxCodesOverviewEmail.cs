using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

public record LockBoxCodesOverviewEmail(EmailAddress Email, string FullName, IEnumerable<WeeklyLockBoxCodes> Codes);
