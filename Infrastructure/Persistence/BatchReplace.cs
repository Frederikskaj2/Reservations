namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

sealed record BatchReplace(Key Key, object Item, string ETag) : BatchOperation;