using System;

namespace Frederikskaj2.Reservations.Importer.Input;

[Flags]
public enum ReservationEmails
{
    None,
    LockBoxCode,
    OverduePayment = LockBoxCode << 1,
    NeedsSettlement = OverduePayment << 1
}