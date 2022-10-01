using Frederikskaj2.Reservations.Shared.Core;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record PayOut(EmailAddress Email, string FullName, Amount Amount) : MessageBase(Email, FullName);
