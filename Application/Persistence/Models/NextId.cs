namespace Frederikskaj2.Reservations.Application;

record NextId(string Name, int Id)
{
    public static string GetId(NextId nextId) => nextId.Name;
}