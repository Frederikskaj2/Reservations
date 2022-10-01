using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared.Core;

[Flags]
[SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "The enum has a zero value that is appropriately named.")]
public enum Roles
{
    None,
    Resident,
    OrderHandling = Resident << 1,
    Bookkeeping = OrderHandling << 1,
    UserAdministration = Bookkeeping << 1,
    Cleaning = UserAdministration << 1,
    LockBoxCodes = Cleaning << 1
}
