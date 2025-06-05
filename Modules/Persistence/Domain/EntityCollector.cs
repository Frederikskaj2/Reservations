using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System;
using System.Linq;

namespace Frederikskaj2.Reservations.Persistence;

public class EntityCollector
{
    readonly HashMap<Key, Entity> entityMap;

    public EntityCollector() : this(HashMap<Key, Entity>.Empty) { }

    EntityCollector(HashMap<Key, Entity> entityMap) => this.entityMap = entityMap;

    public EntityCollector Add<T>(ETaggedEntity<T> entity) where T : class =>
        new(Add(entityMap, entity));

    public EntityCollector Add<T>(Seq<ETaggedEntity<T>> entities) where T : class =>
        new(entities.Fold(entityMap, Add));

    static HashMap<Key, Entity> Add<T>(HashMap<Key, Entity> entityMap, ETaggedEntity<T> entity) where T : class =>
        entityMap.Add(GetKey(entity.Id, entity.Value), new(entity.Value, entity.ETag));

    public EntityCollector TryAdd<T>(OptionalEntity<T> optionalEntity) where T : class =>
        optionalEntity.Match(_ => this, Add);

    public EntityTracker ToTracker() =>
        new(entityMap.Select(tuple => (tuple.Key.Id, tuple.Value.Value, tuple.Value.ETag)));

    static Key GetKey(string id, object value) => new(id, value.GetType());

    record Key(string Id, Type Type);

    record Entity(object Value, ETag ETag);
}
