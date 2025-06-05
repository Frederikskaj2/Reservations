using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Persistence;

[method: JsonConstructor]
class Document<T>(string id, string discriminator, T value, string partitionKey, string? eTag = null)
{
    [JsonPropertyName("id")]
    public string Id { get; } = id;

    [JsonPropertyName("v")]
    public T Value { get; } = value;

    [JsonPropertyName("d")]
    public string Discriminator { get; } = discriminator;

    [JsonPropertyName(Constants.PartitionKeyPropertyName)]
    public string PartitionKey { get; } = partitionKey;

    [JsonPropertyName("_etag")]
    public string? ETag { get; } = eTag;
}
