using System;

namespace Frederikskaj2.Reservations.Shared.Core;

[Flags]
public enum EmailSubscriptions
{
    None,
    NewOrder,
    SettlementRequired = NewOrder << 1,
    CleaningScheduleUpdated = SettlementRequired << 1
}
