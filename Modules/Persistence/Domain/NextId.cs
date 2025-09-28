using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Persistence;

record NextId(string Name, int Id) : IHasId
{
    string IHasId.GetId() => Name;
}
