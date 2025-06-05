using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

public class EntityTracker
{
    readonly HashMap<Key, Entity> entityMap;

    EntityTracker(HashMap<Key, Entity> entityMap) => this.entityMap = entityMap;

    internal EntityTracker(IEnumerable<(string Id, object Value, ETag ETag)> entities) =>
        entityMap = toHashMap(entities.Select(tuple => (new Key(tuple.Id, tuple.Value.GetType()), new Entity(tuple.Value, Operation.None, tuple.ETag))));

    public static EntityTracker CreateEmpty() => new((IEnumerable<(string Id, object Value, ETag ETag)>) []);

    public EntityTracker Add<T>(T value) where T : class, IHasId =>
        new(entityMap.Add(GetKey(value.GetId(), value.GetType()), new(value, Operation.Add, null)));

    public EntityTracker Add<T>(Seq<T> values) where T : class, IHasId =>
        new(values.Fold(entityMap, (map, value) => map.Add(GetKey(value.GetId(), value.GetType()), new(value, Operation.Add, null))));

    public EntityTracker TryAdd<T>(Option<T> valueOption) where T : class, IHasId =>
        valueOption.Case switch
        {
            T value => Add(value),
            _ => this,
        };

    public EntityTracker Update<T>(T value) where T : class, IHasId =>
        new(Update(entityMap, value.GetId(), value));

    public EntityTracker Update<T>(Seq<T> values) where T : class, IHasId =>
        new(values.Fold(entityMap, (hashMap, value) => Update(hashMap, value.GetId(), value)));

    public EntityTracker TryUpdate<T>(Option<T> option) where T : class, IHasId =>
        option.Case switch
        {
            T value => Update(value),
            _ => this,
        };

    public EntityTracker AddOrUpdate<T>(OptionalEntity<T> optionalEntity) where T : class =>
        new(
            optionalEntity.Match(
                entity => Add(entity.Id, entity.Value),
                entity => Update(entityMap, entity.Id, entity.Value)));

    public EntityTracker Upsert<T>(T value) where T : class, IHasId =>
        new(entityMap.Add(GetKey(value.GetId(), value.GetType()), new(value, Operation.Upsert, null)));

    HashMap<Key, Entity> Add<T>(string id, T value) where T : class =>
        entityMap.Add(GetKey(id, value.GetType()), new(value, Operation.Add, null));

    static HashMap<Key, Entity> Update<T>(HashMap<Key, Entity> entityMap, string id, T value) where T : class
    {
        var key = GetKey(id, value.GetType());
        return entityMap.Find(key).Case switch
        {
            Entity { Operation: Operation.None } entity => UpdateIfModified(entityMap, key, entity, value),
            _ => throw new InvalidOperationException(),
        };
    }

    static HashMap<Key, Entity> UpdateIfModified<T>(HashMap<Key, Entity> entities, Key key, Entity entity, T value) where T : class =>
        !Equals(value, entity.Value)
            ? entities.SetItem(key, entity with { Value = value, Operation = Operation.Update })
            : entities;

    public EntityTracker Remove<T>(IIsId id, ETag? eTag) where T : class =>
        new(Remove<T>(entityMap, id.GetId(), eTag?.ToString()));

    static HashMap<Key, Entity> Remove<T>(HashMap<Key, Entity> entityMap, string id, string? eTag) where T : class =>
        entityMap.Add(GetKey(id, typeof(T)), new(Value: null, Operation.Remove, eTag));

    public EntityTracker Remove<T>(ETaggedEntity<T> eTaggedEntity) where T : class =>
        new(Remove(entityMap, eTaggedEntity));

    public EntityTracker Remove<T>(Seq<ETaggedEntity<T>> eTaggedEntities) where T : class =>
        new(eTaggedEntities.Fold(entityMap, Remove));

    static HashMap<Key, Entity> Remove<T>(HashMap<Key, Entity> entityMap, ETaggedEntity<T> eTaggedEntity) where T : class
    {
        var key = GetKey(eTaggedEntity.Id, eTaggedEntity.Value.GetType());
        return entityMap.Find(key).Case switch
        {
            Entity { Operation: Operation.None } entity => entityMap.SetItem(key, entity with { Operation = Operation.Remove }),
            _ => throw new InvalidOperationException(),
        };
    }

    public Seq<EntityOperation> GetOperations() =>
        entityMap.Where(tuple => tuple.Value.Operation is not Operation.None).Select(tuple => CreateOperation(tuple.Key, tuple.Value)).ToSeq();

    static EntityOperation CreateOperation(Key key, Entity entity) =>
        entity.Operation switch
        {
            Operation.Add => new EntityAdd(key.Id, key.Type, entity.Value ?? throw new UnreachableException()),
            Operation.Update => new EntityUpdate(
                key.Id, key.Type, entity.Value ?? throw new UnreachableException(), entity.ETag ?? throw new UnreachableException()),
            Operation.Upsert => new EntityUpsert(key.Id, key.Type, entity.Value ?? throw new UnreachableException()),
            Operation.Remove => new EntityRemove(key.Id, key.Type, entity.ETag ?? throw new UnreachableException()),
            _ => throw new InvalidOperationException(),
        };

    static Key GetKey(string id, Type type) => new(id, type);

    record Key(string Id, Type Type);

    record Entity(object? Value, Operation Operation, ETag? ETag);

    enum Operation
    {
        None,
        Add,
        Update,
        Upsert,
        Remove,
    }
}
