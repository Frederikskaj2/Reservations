namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

sealed record BatchCreate(Key Key, object Item) : BatchOperation;