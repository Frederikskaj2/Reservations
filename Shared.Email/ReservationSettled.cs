using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Email;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Properties are used in Razor template.")]
public record ReservationSettled(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    Uri OrderUrl,
    ReservationDescription Reservation,
    Amount Deposit,
    Amount Damages,
    string? Description) : MessageBase(Email, FullName);
