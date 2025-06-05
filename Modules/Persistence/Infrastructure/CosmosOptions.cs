using System.Collections.Generic;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Persistence;

public class CosmosOptions
{
    const string emulatorConnectionString =
        "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    const string defaultDatabaseId = "Frederikskaj2";

    const string defaultContainerId = "Local";

    const string defaultPartitionKey = "";

    public string ConnectionString { get; init; } = emulatorConnectionString;
    public string DatabaseId { get; set; } = defaultDatabaseId;
    public string ContainerId { get; set; } = defaultContainerId;
    public string PartitionKey { get; init; } = defaultPartitionKey;
    public JsonSerializerOptions SerializerOptions { get; set; } = new();
    public IEnumerable<IEnumerable<IndexPath>>? CompositeIndices { get; init; }
}
