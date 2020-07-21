using System;

namespace Frederikskaj2.Reservations.Server.Data
{
    [Flags]
    public enum ReservationEmails
    {
        None,
        LockBoxCode,
        OverduePayment = LockBoxCode << 1,
        NeedsSettlement = OverduePayment << 1
    }
}