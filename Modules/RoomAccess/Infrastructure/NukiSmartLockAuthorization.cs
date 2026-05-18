using System;

namespace Frederikskaj2.Reservations.RoomAccess;

record NukiSmartLockAuthorization(
    string Id,
    ulong SmartLockId,
    int Code,
    int Type,
    string Name,
    DateTime AllowedFromDate,
    DateTime AllowedUntilDate,
    int AllowedWeekdays);
