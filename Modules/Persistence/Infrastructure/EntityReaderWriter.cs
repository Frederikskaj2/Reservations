using Frederikskaj2.Reservations.Core;
using LanguageExt;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

sealed class EntityReaderWriter(ILogger<EntityReaderWriter> logger, IOptions<CosmosOptions> options) : IEntityReader, IEntityWriter, IDisposable
{
    const char separator = '|';
    readonly SemaphoreSlim gate = new(1, 1);
    readonly QueryRequestOptions queryRequestOptions = new() { PartitionKey = new PartitionKey(options.Value.PartitionKey) };
    readonly JsonSerializerOptions serializerOptions = options.Value.SerializerOptions;
    State? state;

    public void Dispose()
    {
        state?.Client.Dispose();
        gate.Dispose();
    }

    public EitherAsync<HttpStatusCode, ETaggedEntity<T>> ReadWithETag<T>(string id, CancellationToken cancellationToken)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, ETaggedEntity<T>>> ToEither()
        {
            var container = await GetContainerAsync(cancellationToken);
            var type = typeof(T);
            using var response = await container.ReadItemStreamAsync(
                GetFullId(GetDiscriminator(type), id),
                new(options.Value.PartitionKey),
                cancellationToken: cancellationToken);
            logger.LogDebug("Read {Type} with ID '{Id}': Status = {Status}, RU = {RU}", type.Name, id, response.StatusCode, response.Headers.RequestCharge);
            return response.StatusCode.IsSuccess() ? await GetETaggedEntity(response) : response.StatusCode;

            async ValueTask<ETaggedEntity<T>> GetETaggedEntity(ResponseMessage responseMessage)
            {
                var document = await DeserializeAsync<Document<T>>(responseMessage.Content);
                logger.LogTrace("Read {Document}", document.Value);
                return new(GetId(document.Id), document.Value, document.ETag!);
            }
        }
    }

    public EitherAsync<HttpStatusCode, ETaggedEntity<T>> ReadWithETag<T>(IIsId id, CancellationToken cancellationToken) =>
        ReadWithETag<T>(id.GetId(), cancellationToken);

    public EitherAsync<HttpStatusCode, T> Read<T>(string id, CancellationToken cancellationToken) =>
        from entity in ReadWithETag<T>(id, cancellationToken)
        select entity.Value;

    public EitherAsync<HttpStatusCode, T> Read<T>(IIsId id, CancellationToken cancellationToken) =>
        from entity in ReadWithETag<T>(id, cancellationToken)
        select entity.Value;

    public EitherAsync<HttpStatusCode, OptionalEntity<T>> ReadOptional<T>(string id, Func<T> notFoundFactory, CancellationToken cancellationToken) =>
        ReadWithETag<T>(id, cancellationToken).BiBind<OptionalEntity<T>>(
            eTaggedEntity => (OptionalEntity<T>) eTaggedEntity,
            status => status switch
            {
                HttpStatusCode.NotFound => (OptionalEntity<T>) new Entity<T>(id, notFoundFactory()),
                _ => status,
            });

    public EitherAsync<HttpStatusCode, OptionalEntity<T>> ReadOptional<T>(IIsId id, Func<T> notFoundFactory, CancellationToken cancellationToken) =>
        ReadOptional(id.GetId(), notFoundFactory, cancellationToken);

    public EitherAsync<HttpStatusCode, Seq<ETaggedEntity<T>>> QueryWithETag<T>(IQuery<T> query, CancellationToken cancellationToken)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, Seq<ETaggedEntity<T>>>> ToEither()
        {
            var container = await GetContainerAsync(cancellationToken);
            var sql = query.Sql;
            var iterator = container.GetItemQueryStreamIterator(sql, continuationToken: null, queryRequestOptions);
            var items = new Seq<ETaggedEntity<T>>();
            while (iterator.HasMoreResults)
            {
                using var response = await iterator.ReadNextAsync(cancellationToken);
                logger.LogDebug("Query '{Sql}': Status = {Status}, RU = {RU}", sql, response.StatusCode, response.Headers.RequestCharge);
                if (!response.StatusCode.IsSuccess())
                    return response.StatusCode;
                var result = await DeserializeAsync<QueryDocuments<Document<T>>>(response.Content);
                items = items.Concat(result.Documents.Select(document => new ETaggedEntity<T>(GetId(document.Id), document.Value, document.ETag!)));
            }

            logger.LogDebug("Query read {Count} item(s)", items.Count);
            if (items.Count > 0)
                logger.LogTrace("Query read {Items}", items);
            return items;
        }
    }

    public EitherAsync<HttpStatusCode, Seq<T>> Query<T>(IQuery<T> query, CancellationToken cancellationToken)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, Seq<T>>> ToEither()
        {
            var container = await GetContainerAsync(cancellationToken);
            var sql = query.Sql;
            var iterator = container.GetItemQueryStreamIterator(sql, continuationToken: null, queryRequestOptions);
            var items = new Seq<T>();
            while (iterator.HasMoreResults)
            {
                using var response = await iterator.ReadNextAsync(cancellationToken);
                logger.LogDebug("Query '{Sql}': Status = {Status}, RU = {RU}", sql, response.StatusCode, response.Headers.RequestCharge);
                if (!response.StatusCode.IsSuccess())
                    return response.StatusCode;
                var result = await DeserializeAsync<QueryDocuments<T>>(response.Content);
                items = items.Concat(result.Documents);
            }

            logger.LogDebug("Query read {Count} item(s)", items.Count);
            if (items.Count > 0)
                logger.LogTrace("Query read {Items}", items);
            return items;
        }
    }

    public EitherAsync<HttpStatusCode, Seq<(EntityOperation Operation, ETag ETag)>> Write(Seq<EntityOperation> operations, CancellationToken cancellationToken)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, Seq<(EntityOperation Operation, ETag ETag)>>> ToEither()
        {
            if (operations.Count is 0)
                return Seq<(EntityOperation Operation, ETag ETag)>();
            var container = await GetContainerAsync(cancellationToken);
            var batch = await operations.FoldAsync(container.CreateTransactionalBatch(new(options.Value.PartitionKey)), Folder);
            using var response = await batch.ExecuteAsync(cancellationToken);
            logger.LogDebug(
                "Batch with {Count} operations: Status = {Status}, RU = {RU}",
                operations.Count,
                response.StatusCode,
                response.Headers.RequestCharge);
            if (response.IsSuccessStatusCode)
                return response.Zip(operations, (result, operation) => (operation, ETag.FromString(result.ETag))).ToSeq();
            if (logger.IsEnabled(LogLevel.Debug))
                foreach (var (operation, result) in response.Zip(operations, (result, operation) => (Operation: operation, Result: result)))
                    logger.LogDebug("Operation {Operation} had status {Status}", operation.Value.GetType().Name, result.StatusCode);
            return response.StatusCode;
        }
    }

    ValueTask<TransactionalBatch> Folder(TransactionalBatch batch, EntityOperation operation) =>
        operation.Match(
            add => AddCreateItemToBatch(batch, add.Id, add.Type, add.Value),
            update => AddReplaceItemToBatch(batch, update.Id, update.Type, update.Value, update.ETag),
            upsert => AddUpsertItemToBatch(batch, upsert.Id, upsert.Type, upsert.Value),
            remove => AddDeleteItemToBatch(batch, remove.Id, remove.Type, remove.ETag));

    async ValueTask<TransactionalBatch> AddCreateItemToBatch(TransactionalBatch batch, string id, Type type, object value)
    {
        logger.LogDebug("Batch create {Type} with ID '{Id}'", type.Name, id);
        logger.LogTrace("Batch create {Document}", value);
        return batch.CreateItemStream(await SerializeAsync(WrapValue(id, type, value)), CreateBatchRequestOptions(None));
    }

    async ValueTask<TransactionalBatch> AddReplaceItemToBatch(TransactionalBatch batch, string id, Type type, object value, ETag eTag)
    {
        logger.LogDebug("Batch replace {Type} with ID '{Id}' and ETag '{ETag}'", type.Name, id, eTag);
        logger.LogTrace("Batch replace {Document}", value);
        return batch.ReplaceItemStream(GetFullId(id, value), await SerializeAsync(WrapValue(id, type, value)), CreateBatchRequestOptions(eTag));
    }

    async ValueTask<TransactionalBatch> AddUpsertItemToBatch(TransactionalBatch batch, string id, Type type, object value)
    {
        logger.LogDebug("Batch upsert {Type} with ID '{Id}'", type.Name, id);
        logger.LogTrace("Batch upsert {Document}", value);
        return batch.UpsertItemStream(await SerializeAsync(WrapValue(id, type, value)), CreateBatchRequestOptions(None));
    }

    ValueTask<TransactionalBatch> AddDeleteItemToBatch(TransactionalBatch batch, string id, Type type, ETag eTag)
    {
        logger.LogDebug("Batch delete {Type} with ID '{Id}' and ETag '{ETag}'", type.Name, id, eTag);
        return ValueTask.FromResult(batch.DeleteItem(GetFullId(id, type), CreateBatchRequestOptions(eTag)));
    }

    async ValueTask<Container> GetContainerAsync(CancellationToken cancellationToken) => (await GetState(cancellationToken)).Container;

    async ValueTask<Stream> SerializeAsync<T>(T item)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions);
        stream.Position = 0;
        return stream;
    }

    ValueTask<T> DeserializeAsync<T>(Stream stream) =>
        JsonSerializer.DeserializeAsync<T>(stream, serializerOptions)!; // Only used with Cosmos that will never produce "null" JSON.

    static TransactionalBatchItemRequestOptions CreateBatchRequestOptions(Option<ETag> eTag) =>
        new() { EnableContentResponseOnWrite = false, IfMatchEtag = eTag.ToNullable()?.ToString() };

    static string GetId(string fullId)
    {
        var index = fullId.IndexOf(separator, StringComparison.Ordinal);
        return index >= 0 ? fullId[(index + 1)..] : throw new ArgumentException($"Full ID '{fullId}' is not valid.", nameof(fullId));
    }

    static string GetDiscriminator(Type type) => type.Name;

    static string GetFullId(string discriminator, string id) => $"{discriminator}{separator}{id}";

    static string GetFullId(string id, Type type) => GetFullId(GetDiscriminator(type), id);

    static string GetFullId(string id, object value) => GetFullId(id, value.GetType());

    Document<object> WrapValue(string id, Type type, object value)
    {
        var discriminator = GetDiscriminator(type);
        return new(GetFullId(discriminator, id), discriminator, value, options.Value.PartitionKey);
    }

    async ValueTask<State> GetState(CancellationToken cancellationToken)
    {
        try
        {
            await gate.WaitAsync(cancellationToken);
            state ??= await Initialize(cancellationToken);
            return state;
        }
        finally
        {
            gate.Release();
        }
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership and the responsibility to dispose is transferred to this class.")]
    async ValueTask<State> Initialize(CancellationToken cancellationToken)
    {
        var cosmosOptions = options.Value;

        var client = new CosmosClient(cosmosOptions.ConnectionString);
        Database database = await client.CreateDatabaseIfNotExistsAsync(cosmosOptions.DatabaseId, cancellationToken: cancellationToken);
        Container container = await database.CreateContainerIfNotExistsAsync(
            cosmosOptions.ContainerId, $"/{Constants.PartitionKeyPropertyName}", cancellationToken: cancellationToken);
        logger.LogInformation("Connected to Cosmos container {DatabaseId}.{ContainerId} at {Endpoint}", database.Id, container.Id, client.Endpoint);

        await ConfigureCompositeIndices(logger, cosmosOptions, container, cancellationToken);

        return new(client, database, container);

        static async ValueTask ConfigureCompositeIndices(ILogger logger, CosmosOptions options, Container container, CancellationToken cancellationToken)
        {
            if (options.CompositeIndices is null)
                return;

            var response = await container.ReadContainerAsync(cancellationToken: cancellationToken);
            var compositeIndices = response.Resource.IndexingPolicy.CompositeIndexes;

            var existingIndices = new CompositeIndices(
                compositeIndices.Select(
                    index => new CompositeIndex(index.Select(
                        path => new CompositeIndexPath(path.Path!, path.Order is CompositePathSortOrder.Descending)))));

            var updatedIndices = options.CompositeIndices.Aggregate(
                existingIndices,
                (indices, paths) => indices.AddIndex(new(paths.Select(path => new CompositeIndexPath(path.Path!, path.IsDescending)))));

            compositeIndices.Clear();
            foreach (var index in updatedIndices)
                compositeIndices.Add(new(index.Select(path => new CompositePath
                {
                    Path = path.Path, Order = !path.IsDescending ? CompositePathSortOrder.Ascending : CompositePathSortOrder.Descending,
                }).ToList()));

            await container.ReplaceContainerAsync(response.Resource, cancellationToken: cancellationToken);

            logger.LogInformation("Configured composite indices {CompositeIndices}", updatedIndices);
        }
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    record State(CosmosClient Client, Database Database, Container Container);
}
