namespace Frederikskaj2.Reservations.Application;

public interface IProjectedQuery<TDocument>
{
    string Sql { get; }
}
