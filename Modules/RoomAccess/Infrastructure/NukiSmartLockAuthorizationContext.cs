using LanguageExt;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

class NukiSmartLockAuthorizationContext(HashMap<SmartLockAuthorization, string> toDelete, Seq<SmartLockAuthorization> toAdd) : ISmartLockAuthorizationContext
{
    public NukiSmartLockAuthorizationContext(IEnumerable<(SmartLockAuthorization Authorization, string Id)> authorizations)
        : this(toHashMap(authorizations), Empty)
    {
    }

    public ISmartLockAuthorizationContext AddAuthorizations(IEnumerable<SmartLockAuthorization> authorizations)
    {
        var (newToDelete, newToAdd) = authorizations.Fold(
            (ToDelete: toDelete, ToAdd: toAdd),
            (tuple, authorization) => tuple.ToDelete.Find(authorization).Case switch
            {
                string => (tuple.ToDelete.Remove(authorization), tuple.ToAdd),
                _ => (tuple.ToDelete, tuple.ToAdd.Add(authorization)),
            });
        return new NukiSmartLockAuthorizationContext(newToDelete, newToAdd);
    }

    public IEnumerable<(SmartLockAuthorization Authorization, string Id)> ToDelete => toDelete.Map(tuple => (tuple.Key, tuple.Value));

    public IEnumerable<SmartLockAuthorization> AuthorizationsToAdd => toAdd;
}
