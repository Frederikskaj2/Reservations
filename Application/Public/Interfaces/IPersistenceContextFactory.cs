namespace Frederikskaj2.Reservations.Application;

public interface IPersistenceContextFactory
{
    IPersistenceContext Create(string partitionKey);
}
