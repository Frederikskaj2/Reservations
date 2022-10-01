using Frederikskaj2.Reservations.Infrastructure;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public sealed class DatabaseInitializer : IDisposable
{
    const string partitionKeyValue = "";
    const string partitionKeyPropertyName = "pk";
    readonly JsonSerializerOptions serializerOptions;
    readonly AsyncLazy<State> state;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger, IOptions<CosmosOptions> options)
    {
        serializerOptions = options.Value.SerializerOptions;
        state = new AsyncLazy<State>(() => InitializeAsync(logger, options.Value));
    }

    public void Dispose()
    {
        if (state.IsValueCreated)
            state.Value.Result.Client.Dispose();
    }

    public async Task<ResponseMessage> CreateAsync<T>(string id, T item, string discriminator) where T : class
    {
        var container = await GetContainerAsync();
        var document = WrapValue(id, item, discriminator);
        var stream = await SerializeAsync(document);
        using var response = await container.CreateItemStreamAsync(stream, new PartitionKey(partitionKeyValue));
        return response;
    }

    static Document<T> WrapValue<T>(string id, T value, string discriminator) where T : class =>
        new(GetFullId(discriminator, id), discriminator, value);

    static string GetFullId(string discriminator, string id) => $"{discriminator}|{id}";

    async ValueTask<Stream> SerializeAsync<T>(T item)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions);
        stream.Position = 0;
        return stream;
    }

    async ValueTask<Container> GetContainerAsync()
    {
        var value = await state.Value;
        return value.Container;
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership and the responsibility to dispose is transferred to the creator.")]
    static async Task<State> InitializeAsync(ILogger logger, CosmosOptions options)
    {
        logger.LogInformation("Creating seed data in database {Database}, container {Container}", options.DatabaseId, options.ContainerId);
        var client = new CosmosClient(options.ConnectionString);
        const int freeTierDatabaseThroughput = 1000;
        Database database = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId, freeTierDatabaseThroughput);
        Container container = await database.CreateContainerIfNotExistsAsync(options.ContainerId, $"/{partitionKeyPropertyName}");
        return new State(client, database, container);
    }

    record State(CosmosClient Client, Database Database, Container Container);

    class Document<T> where T : class
    {
        public Document(string id, string discriminator, T value, string partitionKey = partitionKeyValue) =>
            (Id, Discriminator, Value, PartitionKey) = (id, discriminator, value, partitionKey);

        [JsonPropertyName("id")] public string Id { get; }

        [JsonPropertyName("v")] public T Value { get; }

        [JsonPropertyName("d")] public string Discriminator { get; }

        [JsonPropertyName(partitionKeyPropertyName)]
        public string PartitionKey { get; }
    }
}
