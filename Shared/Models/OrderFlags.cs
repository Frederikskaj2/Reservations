using System;

namespace Frederikskaj2.Reservations.Shared
{
    [Flags]
    public enum OrderFlags
    {
        None,
        IsCleaningRequired,
        IsHistoryOrder = IsCleaningRequired << 1,
        IsOwnerOrder = IsHistoryOrder << 1
    }
}
