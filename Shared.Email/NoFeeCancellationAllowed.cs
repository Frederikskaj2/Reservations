using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Shared.Email;

public record NoFeeCancellationAllowed(EmailAddress Email, string FullName, OrderId OrderId, Uri OrderUrl, Period Duration) : MessageBase(Email, FullName);
