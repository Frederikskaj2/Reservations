using System;

namespace Frederikskaj2.Reservations.Importer.Input;

[Flags]
public enum EmailSubscriptions
{
    None,
    NewOrder,
    OverduePayment = NewOrder << 1,
    SettlementRequired = OverduePayment << 1,
    CleaningScheduleUpdated = SettlementRequired << 1
}
