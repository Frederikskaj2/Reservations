using Frederikskaj2.Reservations.Shared.Core;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record PayIn(EmailAddress Email, string FullName, Amount Amount, PaymentInformation? Payment) : MessageBase(Email, FullName);
