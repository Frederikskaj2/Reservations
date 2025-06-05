using System;

namespace Frederikskaj2.Reservations.Core;

[Flags]
public enum Roles
{
    None,
    Resident,
    OrderHandling = Resident << 1,
    Bookkeeping = OrderHandling << 1,
    UserAdministration = Bookkeeping << 1,
    Cleaning = UserAdministration << 1,
    LockBoxCodes = Cleaning << 1,
    Jobs = LockBoxCodes << 1,
}
