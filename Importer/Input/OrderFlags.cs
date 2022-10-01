using System;

namespace Frederikskaj2.Reservations.Importer.Input;

[Flags]
public enum OrderFlags
{
    None,
    IsCleaningRequired,
    IsHistoryOrder = IsCleaningRequired << 1,
    IsOwnerOrder = IsHistoryOrder << 1
}