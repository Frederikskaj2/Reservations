using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class QueryDocuments<T>
{
    [JsonConstructor]
    public QueryDocuments(IEnumerable<T> documents, int count) => (Documents, Count) = (documents, count);

    [JsonPropertyName("Documents")]
    public IEnumerable<T> Documents { get; }

    [JsonPropertyName("_count")]
    public int Count { get; }
}
