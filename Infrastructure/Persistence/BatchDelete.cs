namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

sealed record BatchDelete(Key Key, string ETag) : BatchOperation;