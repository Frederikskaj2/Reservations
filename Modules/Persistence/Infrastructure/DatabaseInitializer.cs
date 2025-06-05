using Frederikskaj2.Reservations.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Persistence;

public sealed class DatabaseInitializer(ILogger<DatabaseInitializer> logger, IOptions<CosmosOptions> options) : IDisposable
{
    const string partitionKey = "";

    readonly JsonSerializerOptions serializerOptions = options.Value.SerializerOptions;
    readonly AsyncLazy<State> state = new(() => Initialize(logger, options.Value));

    public void Dispose()
    {
        if (state.IsValueCreated)
            state.Value.GetAwaiter().GetResult().Client.Dispose();
    }

    public async Task<ResponseMessage> Create<T>(string id, T item, string discriminator) where T : class
    {
        var container = await GetContainer();
        var document = WrapValue(id, item, discriminator);
        var stream = await Serialize(document);
        return await container.CreateItemStreamAsync(stream, new(partitionKey));
    }

    static Document<T> WrapValue<T>(string id, T value, string discriminator) where T : class =>
        new(GetFullId(discriminator, id), discriminator, value);

    static string GetFullId(string discriminator, string id) => $"{discriminator}|{id}";

    async ValueTask<Stream> Serialize<T>(T item)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions);
        stream.Position = 0;
        return stream;
    }

    async ValueTask<Container> GetContainer()
    {
        var value = await state.Value;
        return value.Container;
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership and the responsibility to dispose is transferred to the creator.")]
    static async Task<State> Initialize(ILogger logger, CosmosOptions options)
    {
        logger.LogInformation("Creating seed data in database {Database}, container {Container}", options.DatabaseId, options.ContainerId);
        var client = new CosmosClient(options.ConnectionString);
        const int freeTierDatabaseThroughput = 1000;
        Database database = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId, freeTierDatabaseThroughput);
        Container container = await database.CreateContainerIfNotExistsAsync(options.ContainerId, $"/{Constants.PartitionKeyPropertyName}");
        return new(client, database, container);
    }

    record State(CosmosClient Client, Database Database, Container Container);

    class Document<T>(string id, string discriminator, T value, string partitionKey = partitionKey) where T : class
    {
        [JsonPropertyName("id")] public string Id { get; } = id;

        [JsonPropertyName("v")] public T Value { get; } = value;

        [JsonPropertyName("d")] public string Discriminator { get; } = discriminator;

        [JsonPropertyName(Constants.PartitionKeyPropertyName)]
        public string PartitionKey { get; } = partitionKey;
    }
}
