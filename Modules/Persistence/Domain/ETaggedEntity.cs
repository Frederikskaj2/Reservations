using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Persistence;

public record ETaggedEntity<T>(string Id, T Value, ETag ETag);
