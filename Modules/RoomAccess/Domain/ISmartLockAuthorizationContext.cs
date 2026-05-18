using System.Collections.Generic;

namespace Frederikskaj2.Reservations.RoomAccess;

public interface ISmartLockAuthorizationContext
{
    ISmartLockAuthorizationContext AddAuthorizations(IEnumerable<SmartLockAuthorization> authorizations);
}
