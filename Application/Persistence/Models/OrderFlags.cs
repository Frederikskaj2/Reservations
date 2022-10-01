using System;

namespace Frederikskaj2.Reservations.Application;

[Flags]
enum OrderFlags
{
    None,
    IsCleaningRequired,
    IsHistoryOrder = IsCleaningRequired << 1,
    IsOwnerOrder = IsHistoryOrder << 1
}