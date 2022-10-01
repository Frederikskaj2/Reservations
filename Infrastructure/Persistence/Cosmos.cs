using Frederikskaj2.Reservations.Application;
using LanguageExt;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

sealed class Cosmos : IPersistenceContextFactory, IDisposable
{
    const char separator = '|';
    readonly ILogger logger;
    readonly JsonSerializerOptions serializerOptions;
    readonly AsyncLazy<State> state;

    public Cosmos(ILogger<Cosmos> logger, IOptions<CosmosOptions> options)
    {
        this.logger = logger;
        serializerOptions = options.Value.SerializerOptions;
        state = new AsyncLazy<State>(() => InitializeAsync(logger, options));
    }

    public void Dispose()
    {
        if (state.IsValueCreated)
            state.Value.GetAwaiter().GetResult().Client.Dispose();
    }

    public IPersistenceContext Create(string partitionKey) => new CosmosContext(this, partitionKey);

    public async ValueTask<ETaggedResult<T>> ReadAsync<T>(string id, string partitionKey)
    {
        var container = await GetContainerAsync();
        var type = typeof(T);
        using var response = await container.ReadItemStreamAsync(GetFullId(GetDiscriminator(type), id), new PartitionKey(partitionKey));
        logger.LogDebug("Read {Type} with ID '{Id}': Status = {Status}, RU = {RU}", type.Name, id, response.StatusCode, response.Headers.RequestCharge);
        return response.StatusCode.IsSuccess() ? await GetETagged() : new ETaggedResult<T>(response.StatusCode);

        async ValueTask<ETaggedResult<T>> GetETagged()
        {
            var document = await DeserializeAsync<Document<T>>(response.Content);
            logger.LogTrace("Read {Document}", document.Value);
            return new ETaggedResult<T>(response.StatusCode, ETagged.Create(GetId(document.Id), document.Value, document.ETag!));
        }
    }

    public async ValueTask<ETaggedResults<T>> QueryAsync<T>(string sql, string partitionKey)
    {
        var container = await GetContainerAsync();
        var iterator = container.GetItemQueryStreamIterator(sql, null, new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) });
        var items = new List<ETagged<T>>();
        while (iterator.HasMoreResults)
        {
            using var response = await iterator.ReadNextAsync();
            logger.LogDebug("Query '{Sql}': Status = {Status}, RU = {RU}", sql, response.StatusCode, response.Headers.RequestCharge);
            if (!response.StatusCode.IsSuccess())
                return new ETaggedResults<T>(response.StatusCode);
            var result = await DeserializeAsync<QueryDocuments<Document<T>>>(response.Content);
            items.AddRange(result.Documents.Select(document => ETagged.Create(GetId(document.Id), document.Value, document.ETag!)));
        }

        logger.LogDebug("Query read {Count} item(s)", items.Count);
        if (items.Count > 0)
            logger.LogTrace("Query read {Items}", items);
        return new ETaggedResults<T>(items);
    }

    public async ValueTask<Results<T>> QueryProjectedAsync<T>(string sql, string partitionKey)
    {
        var container = await GetContainerAsync();
        var iterator = container.GetItemQueryStreamIterator(sql, null, new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) });
        var items = new List<T>();
        while (iterator.HasMoreResults)
        {
            using var response = await iterator.ReadNextAsync();
            logger.LogDebug("Query '{Sql}': Status = {Status}, RU = {RU}", sql, response.StatusCode, response.Headers.RequestCharge);
            if (!response.StatusCode.IsSuccess())
                return new Results<T>(response.StatusCode);
            var result = await DeserializeAsync<QueryDocuments<T>>(response.Content);
            items.AddRange(result.Documents);
        }
        logger.LogDebug("Query read {Count} item(s)", items.Count);
        if (items.Count > 0)
            logger.LogTrace("Query read {Items}", items);
        return new Results<T>(items);
    }

    public async ValueTask<HttpStatusCode> WriteAsync(string partitionKey, IEnumerable<BatchOperation> operations)
    {
        var count = operations.Count();
        if (count is 0)
            return HttpStatusCode.OK;
        var container = await GetContainerAsync();
        var batch = await operations.FoldAsync(
            container.CreateTransactionalBatch(new PartitionKey(partitionKey)),
            (value, operation) => FolderAsync(value, operation, partitionKey));
        using var response = await batch.ExecuteAsync();
        logger.LogDebug(
            "Batch with {Count} operations: Status = {Status}, RU = {RU}",
            count,
            response.StatusCode,
            response.Headers.RequestCharge);
        if (!response.IsSuccessStatusCode && logger.IsEnabled(LogLevel.Debug))
            foreach (var (operation, result) in response.Zip(operations, (result, operation) => (Operation: operation, Result: result)))
                logger.LogDebug("Operation {Operation} had status {Status}", operation.GetType().Name, result.StatusCode);
        return response.StatusCode;
    }

    async ValueTask<TransactionalBatch> FolderAsync(TransactionalBatch batch, BatchOperation operation, string partitionKey) =>
        operation switch
        {
            BatchCreate(var key, var item) => await AddCreateItemToBatch(batch, partitionKey, key, item),
            BatchReplace(var key, var item, var eTag) => await AddReplaceItemToBatch(batch, partitionKey, key, item, eTag),
            BatchDelete(var key, var eTag) => AddDeleteItemToBatch(batch, key, eTag),
            _ => throw new ArgumentException("Invalid batch operation.", nameof(batch))
        };

    async Task<TransactionalBatch> AddCreateItemToBatch(TransactionalBatch batch, string partitionKey, Key key, object item)
    {
        logger.LogDebug("Batch create {Type} with ID '{Id}'", key.Type.Name, key.Id);
        logger.LogTrace("Batch create {Document}", item);
        return batch.CreateItemStream(await SerializeAsync(WrapValue(key, partitionKey, item)), CreateBatchRequestOptions(None));
    }

    async Task<TransactionalBatch> AddReplaceItemToBatch(TransactionalBatch batch, string partitionKey, Key key, object item, string eTag)
    {
        logger.LogDebug("Batch replace {Type} with ID '{Id}' and ETag '{ETag}'", key.Type.Name, key.Id, eTag);
        logger.LogTrace("Batch replace {Document}", item);
        return batch.ReplaceItemStream(GetFullId(key), await SerializeAsync(WrapValue(key, partitionKey, item)), CreateBatchRequestOptions(eTag));
    }

    TransactionalBatch AddDeleteItemToBatch(TransactionalBatch batch, Key key, string eTag)
    {
        logger.LogDebug("Batch delete {Type} with ID '{Id}' and ETag '{ETag}'", key.Type.Name, key.Id, eTag);
        return batch.DeleteItem(GetFullId(key), CreateBatchRequestOptions(eTag));
    }

    async ValueTask<Container> GetContainerAsync()
    {
        var value = await state.Value;
        return value.Container;
    }

    async ValueTask<Stream> SerializeAsync<T>(T item)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions);
        stream.Position = 0;
        return stream;
    }

    ValueTask<T> DeserializeAsync<T>(Stream stream) =>
        JsonSerializer.DeserializeAsync<T>(stream, serializerOptions)!; // Only used with Cosmos that will never produce "null" JSON.

    static TransactionalBatchItemRequestOptions CreateBatchRequestOptions(Option<string> eTag) =>
        new()
        {
            EnableContentResponseOnWrite = false,
            IfMatchEtag = eTag.Case switch
            {
                string value => value,
                _ => null
            }
        };

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership and the responsibility to dispose is transferred to this class.")]
    static async Task<State> InitializeAsync(ILogger logger, IOptions<CosmosOptions> options)
    {
        var value = options.Value;

        var client = new CosmosClient(value.ConnectionString);
        Database database = await client.CreateDatabaseIfNotExistsAsync(value.DatabaseId);
        Container container = await database.CreateContainerIfNotExistsAsync(value.ContainerId, $"/{Constants.PartitionKeyPropertyName}");
        logger.LogInformation("Connected to Cosmos container {DatabaseId}.{ContainerId} at {Endpoint}", database.Id, container.Id, client.Endpoint);

        await ConfigureCompositeIndices(logger, value, container);

        return new State(client, database, container);

        static async ValueTask ConfigureCompositeIndices(ILogger logger, CosmosOptions options, Container container)
        {
            if (options.CompositeIndices is null)
                return;

            var response = await container.ReadContainerAsync();
            var compositeIndices = response.Resource.IndexingPolicy.CompositeIndexes;

            var existingIndices = new CompositeIndices(
                compositeIndices.Select(
                    index => new CompositeIndex(index.Select(
                        path => new CompositeIndexPath(path.Path!, path.Order is CompositePathSortOrder.Descending)))));

            var updatedIndices = options.CompositeIndices.Aggregate(
                existingIndices,
                (indices, paths) => indices.AddIndex(new CompositeIndex(paths.Select(path => new CompositeIndexPath(path.Path!, path.IsDescending)))));

            compositeIndices.Clear();
            foreach (var index in updatedIndices)
                compositeIndices.Add(new Collection<CompositePath>(index.Select(path => new CompositePath
                {
                    Path = path.Path,
                    Order = !path.IsDescending ? CompositePathSortOrder.Ascending : CompositePathSortOrder.Descending
                }).ToList()));

            await container.ReplaceContainerAsync(response.Resource);

            logger.LogInformation("Configured composite indices {CompositeIndices}", updatedIndices);
        }
    }

    static string GetId(string fullId)
    {
        var index = fullId.IndexOf(separator, StringComparison.Ordinal);
        return index >= 0 ? fullId[(index + 1)..] : throw new ArgumentException($"Full ID '{fullId}' is not valid.", nameof(fullId));
    }

    static string GetDiscriminator(Type type) => type.Name;

    static string GetFullId(string discriminator, string id) => $"{discriminator}{separator}{id}";

    static string GetFullId(Key key) => GetFullId(GetDiscriminator(key.Type), key.Id);

    static Document<object> WrapValue(Key key, string partitionKey, object value)
    {
        var discriminator = GetDiscriminator(key.Type);
        return new Document<object>(GetFullId(discriminator, key.Id), discriminator, value, partitionKey);
    }

    record State(CosmosClient Client, Database Database, Container Container);
}
