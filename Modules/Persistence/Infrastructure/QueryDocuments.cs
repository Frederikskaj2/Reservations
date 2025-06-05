using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Persistence;

[method: JsonConstructor]
class QueryDocuments<T>(IEnumerable<T> documents, int count)
{
    [JsonPropertyName("Documents")]
    public IEnumerable<T> Documents { get; } = documents;

    [JsonPropertyName("_count")]
    public int Count { get; } = count;
}
