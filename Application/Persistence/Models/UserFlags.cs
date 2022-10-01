using System;

namespace Frederikskaj2.Reservations.Application;

[Flags]
enum UserFlags
{
    None,
    IsPendingDelete,
    IsDeleted = IsPendingDelete << 1
}