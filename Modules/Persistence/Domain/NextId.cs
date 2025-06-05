using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Persistence;

record NextId(string Name, int Id) : IHasId
{
    public static string GetId(NextId nextId) => nextId.Name;

    string IHasId.GetId() => Name;
}
