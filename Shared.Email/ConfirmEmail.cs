using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record ConfirmEmail(EmailAddress Email, string FullName, Uri Url, Duration ConfirmEmailDuration) : MessageBase(Email, FullName);
