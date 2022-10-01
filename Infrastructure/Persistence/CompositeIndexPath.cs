namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record CompositeIndexPath(string Path, bool IsDescending)
{
    public override string ToString() => $"{Path}{(IsDescending ? " desc" : "")}";
}
