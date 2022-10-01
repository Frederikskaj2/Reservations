using Frederikskaj2.Reservations.Application;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class CosmosContext : IPersistenceContext
{
    internal CosmosContext(Cosmos cosmos, string partitionKey) : this(cosmos, partitionKey, LanguageExt.HashMap<Key, Entity>.Empty) { }

    CosmosContext(Cosmos cosmos, string partitionKey, HashMap<Key, Entity> entities)
        => (Cosmos, PartitionKey, Entities, Untracked) = (cosmos, partitionKey, entities, new Untracked(this));

    public IPersistenceContextFactory Factory => Cosmos;
    public IUntracked Untracked { get; }
    internal Cosmos Cosmos { get; }
    internal string PartitionKey { get; }
    HashMap<Key, Entity> Entities { get; }

    public T Item<T>() => (T) Entities.Values.Single(entity => entity.Item.GetType() == typeof(T) && entity.Status != EntityStatus.Deleted).Item;

    public T Item<T>(string id) => Entities.Find(GetKey<T>(id)).Case switch
    {
        Entity { Status: EntityStatus.Deleted } => throw new ArgumentException($"Entity with ID '{id}' has been deleted.", nameof(id)),
        Entity entity => entity.Item.GetType() == typeof(T)
            ? (T) entity.Item
            : throw new ArgumentException($"Entity with ID '{id}' does not have type {typeof(T).FullName}.", nameof(id)),
        _ => throw new ArgumentException($"Entity with ID '{id}' does not exist.", nameof(id))
    };

    public Option<T> ItemOption<T>() => Entities.Values
        .Find(entity => entity.Item.GetType() == typeof(T) && entity.Status != EntityStatus.Deleted)
        .Map(entity => (T) entity.Item);

    public Option<T> ItemOption<T>(string id) => Entities.Find(GetKey<T>(id)).Case switch
    {
        Entity { Status: EntityStatus.Deleted } => None,
        Entity entity => entity.Item.GetType() == typeof(T)
            ? (T) entity.Item
            : None,
        _ => None
    };

    public IEnumerable<T> Items<T>() =>
        Entities.Values.Filter(entity => entity.Item.GetType() == typeof(T) && entity.Status != EntityStatus.Deleted).Map(entity => (T) entity.Item);

    public IPersistenceContext CreateItem<T>(string id, T item) where T : class =>
        new CosmosContext(Cosmos, PartitionKey, Entities.Add(GetKey<T>(id), new Entity(item, EntityStatus.New, None)));

    public IPersistenceContext CreateItem<T>(T item, Func<T, string> idFunc) where T : class =>
        CreateItem(idFunc(item), item);

    public IPersistenceContext CreateItems<T>(IEnumerable<T> items, Func<T, string> idFunc) where T : class =>
        items.Fold((IPersistenceContext) this, (context, item) => context.CreateItem(item, idFunc));

    public IPersistenceContext UpdateItem<T>(Func<T, T> transformer) where T : class
    {
        var (key, entity) = Entities.Single(tuple => tuple.Key.Type == typeof(T));
        return UpdateItem(key, transformer((T) entity.Item));
    }

    public IPersistenceContext UpdateItem<T>(string id, Func<T, T> transformer) where T : class
    {
        var key = GetKey<T>(id);
        return Entities.Find(key).Case switch
        {
            Entity { Status: EntityStatus.Deleted } => throw new ArgumentException($"Entity with ID '{id}' has been deleted.", nameof(id)),
            Entity entity => UpdateItem(key, transformer((T) entity.Item)),
            _ => throw new ArgumentException($"Entity with type '{key.Type.Name}' and ID '{id}' does not exist.", nameof(id))
        };
    }

    public IPersistenceContext UpdateItem<T>(string id, T item) where T : class
    {
        var key = GetKey<T>(id);
        return UpdateItem(key, item);
    }

    public IPersistenceContext UpdateItems<T>(Func<T, T> transformer) where T : class =>
        GetEntities<T>().Fold(this, (context, tuple) => context.UpdateItem(tuple.Key, transformer((T) tuple.Entity.Item)));

    public IPersistenceContext UpdateItems<T>(IEnumerable<string> ids, Func<T, T> transformer) where T : class =>
        ids.Fold((IPersistenceContext) this, (context, id) => context.UpdateItem(id, transformer));

    public IPersistenceContext DeleteItem<T>() where T : class =>
        DeleteItem<T>(Entities.Keys.Single(key => key.Type == typeof(T)).Id);

    public IPersistenceContext DeleteItem<T>(string id) where T : class
    {
        var (item, eTag) = GetItemAndETag();
        return new CosmosContext(Cosmos, PartitionKey, Entities.SetItem(GetKey<T>(id), new Entity(item, EntityStatus.Deleted, eTag)));

        (object Item, Option<string> ETag) GetItemAndETag() => Entities.Find(GetKey<T>(id)).Case switch
        {
            Entity { Status: EntityStatus.New } => throw new ArgumentException($"Cannot delete new entity with ID '{id}'.", nameof(id)),
            Entity entity => (entity.Item, entity.ETag),
            _ => throw new ArgumentException($"Cannot delete unknown entity with ID '{id}'.", nameof(id))
        };
    }

    public EitherAsync<HttpStatusCode, IPersistenceContext> ReadItem<T>(string id) where T : class =>
        Add(Cosmos.ReadAsync<T>(id, PartitionKey), id);

    public EitherAsync<HttpStatusCode, IPersistenceContext> ReadItem<T>(string id, Func<T> notFoundFactory) where T : class =>
        AddOrCreate(Cosmos.ReadAsync<T>(id, PartitionKey), id, notFoundFactory);

    public EitherAsync<HttpStatusCode, IPersistenceContext> ReadItems<T>(IQuery<T> query) where T : class =>
        AddRange(Cosmos.QueryAsync<T>(query.Sql, PartitionKey));

    public EitherAsync<HttpStatusCode, IPersistenceContext> Write() =>
        MapHttpError(Cosmos.WriteAsync(PartitionKey, GetBatch()));

    public IQuery<TDocument> Query<TDocument>() where TDocument : class =>
        new CosmosQuery<TDocument>(new CosmosQueryDefinition(false, null, null, null, $@"root.d = ""{typeof(TDocument).Name}""", null));

    EitherAsync<HttpStatusCode, IPersistenceContext> MapHttpError(ValueTask<HttpStatusCode> task)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IPersistenceContext>> ToEither()
        {
            var status = await task;
            return status.IsSuccess() ? this : status;
        }
    }

    EitherAsync<HttpStatusCode, IPersistenceContext> Add<T>(ValueTask<ETaggedResult<T>> task, string id) where T : class
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IPersistenceContext>> ToEither()
        {
            var (status, eTagged) = await task;
            return status switch
            {
                < (HttpStatusCode) 300 => Either<HttpStatusCode, IPersistenceContext>.Right(AddItem(id, eTagged.ValueUnsafe())),
                _ => status
            };
        }
    }

    EitherAsync<HttpStatusCode, IPersistenceContext> AddOrCreate<T>(ValueTask<ETaggedResult<T>> task, string id, Func<T> notFoundFactory) where T : class
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IPersistenceContext>> ToEither()
        {
            var (status, eTagged) = await task;
            return status switch
            {
                HttpStatusCode.NotFound => Either<HttpStatusCode, IPersistenceContext>.Right(CreateItem(id, notFoundFactory())),
                < (HttpStatusCode) 300 => Either<HttpStatusCode, IPersistenceContext>.Right(AddItem(id, eTagged.ValueUnsafe())),
                _ => status
            };
        }
    }

    EitherAsync<HttpStatusCode, IPersistenceContext> AddRange<T>(ValueTask<ETaggedResults<T>> task) where T : class
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IPersistenceContext>> ToEither()
        {
            var (status, items) = await task;
            return status switch
            {
                < (HttpStatusCode) 300 => Either<HttpStatusCode, IPersistenceContext>.Right(AddItems(items)),
                _ => status
            };
        }
    }

    IEnumerable<(Key Key, Entity Entity)> GetEntities<T>() => Entities.Filter(tuple => tuple.Key.Type == typeof(T));

    IPersistenceContext AddItem<T>(string id, ETagged<T> eTagged) where T : class =>
        new CosmosContext(Cosmos, PartitionKey, Entities.AddOrUpdate(GetKey<T>(id), new Entity(eTagged.Item, EntityStatus.Unchanged, eTagged.ETag)));

    IPersistenceContext AddItems<T>(IEnumerable<ETagged<T>> items) where T : class =>
        new CosmosContext(
            Cosmos,
            PartitionKey,
            Entities.AddOrUpdateRange(items.Select(eTagged => (GetKey<T>(eTagged.Id), new Entity(eTagged.Item, EntityStatus.Unchanged, eTagged.ETag)))));

    CosmosContext UpdateItem<T>(Key key, T item) where T : class =>
        Entities.Find(key).Case switch
        {
            Entity { Status: EntityStatus.Deleted } =>
                throw new ArgumentException($"Cannot update deleted entity of type '{key.Type.Name}' with ID '{key.Id}'.", nameof(key)),
            Entity { Status: EntityStatus.New } entity =>
                new CosmosContext(Cosmos, PartitionKey, Entities.SetItem(key, new Entity(item, EntityStatus.New, entity.ETag))),
            Entity entity => new CosmosContext(Cosmos, PartitionKey, Entities.SetItem(key, new Entity(item, EntityStatus.Updated, entity.ETag))),
            _ => throw new ArgumentException($"Entity with type '{key.Type.Name}' and ID '{key.Id}' does not exist.", nameof(key)),
        };

    IEnumerable<BatchOperation> GetBatch() =>
        Entities.Filter(tuple => tuple.Item2.Status != EntityStatus.Unchanged).Map(CreateBatchOperation);

    static BatchOperation CreateBatchOperation((Key Key, Entity Value) tuple) =>
        tuple.Value.Status switch
        {
            EntityStatus.Updated => new BatchReplace(tuple.Key, tuple.Value.Item, tuple.Value.ETag.ValueUnsafe()),
            EntityStatus.New => new BatchCreate(tuple.Key, tuple.Value.Item),
            EntityStatus.Deleted => new BatchDelete(tuple.Key, tuple.Value.ETag.ValueUnsafe()),
            _ => throw new ArgumentException($"Invalid entity status '{tuple.Value.Status}.", nameof(tuple))
        };

    static Key GetKey<T>(string id) => new(id, typeof(T));
}
