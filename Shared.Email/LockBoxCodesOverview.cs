using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record LockBoxCodesOverview(
    EmailAddress Email,
    string FullName,
    IEnumerable<Resource> Resources,
    IEnumerable<WeeklyLockBoxCodes> Codes) : MessageBase(Email, FullName);
