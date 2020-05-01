using System;

namespace Frederikskaj2.Reservations.Server.Data
{
    [Flags]
    public enum ReservationEmails
    {
        None,
        KeyCode,
        OverduePayment = KeyCode << 1,
        NeedsSettlement = OverduePayment << 1
    }
}