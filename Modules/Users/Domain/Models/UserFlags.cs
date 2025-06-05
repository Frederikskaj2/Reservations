using System;

namespace Frederikskaj2.Reservations.Users;

[Flags]
public enum UserFlags
{
    None,
    IsPendingDelete,
    IsDeleted = IsPendingDelete << 1,
}
