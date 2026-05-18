using System;

namespace Frederikskaj2.Reservations.Orders;

[Flags]
public enum ReservationEmails
{
    None,
    RoomEntryCode,
    NeedsSettlement = RoomEntryCode << 1,
}
