using System;

namespace Frederikskaj2.Reservations.Application;

[Flags]
enum ReservationEmails
{
    None,
    LockBoxCode,
    NeedsSettlement = LockBoxCode << 1
}
