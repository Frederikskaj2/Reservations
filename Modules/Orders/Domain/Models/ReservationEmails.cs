using System;

namespace Frederikskaj2.Reservations.Orders;

[Flags]
public enum ReservationEmails
{
    None,
    LockBoxCode,
    NeedsSettlement = LockBoxCode << 1,
}
