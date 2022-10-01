using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record ReservationsCancelled(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    Uri OrderUrl,
    IEnumerable<ReservationDescription> Reservations,
    Amount Refund,
    Amount Fee) : MessageBase(Email, FullName);
