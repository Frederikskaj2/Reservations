using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class Document<T>
{
    public Document(string id, string discriminator, T value, string partitionKey)
        => (Id, Discriminator, Value, PartitionKey) = (id, discriminator, value, partitionKey);

    [JsonConstructor]
    public Document(string id, string discriminator, T value, string partitionKey, string eTag)
        => (Id, Discriminator, Value, PartitionKey, ETag) = (id, discriminator, value, partitionKey, eTag);

    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("v")]
    public T Value { get; }

    [JsonPropertyName("d")]
    public string Discriminator { get; }

    [JsonPropertyName(Constants.PartitionKeyPropertyName)]
    public string PartitionKey { get; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; }
}
