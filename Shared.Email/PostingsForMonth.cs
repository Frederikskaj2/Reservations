using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record PostingsForMonth(
    EmailAddress Email,
    string FullName,
    LocalDate Month,
    IEnumerable<AccountName> AccountNames,
    IEnumerable<Posting> Postings) : MessageBase(Email, FullName);
