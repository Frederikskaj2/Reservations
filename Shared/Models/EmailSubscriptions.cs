using System;

namespace Frederikskaj2.Reservations.Shared
{
    [Flags]
    public enum EmailSubscriptions
    {
        None,
        NewOrder,
        OverduePayment = NewOrder << 1,
        SettlementRequired = OverduePayment << 1,
        CleaningScheduleUpdated = SettlementRequired << 1
    }
}