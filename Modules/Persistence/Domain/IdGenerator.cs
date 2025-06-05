using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

public static class IdGenerator
{
    const int retryCount = 10;

    public static EitherAsync<Failure<Unit>, int> CreateId(IEntityReader reader, IEntityWriter writer, string name, CancellationToken cancellationToken) =>
        CreateId(reader, writer, name, 1, cancellationToken);

    static EitherAsync<Failure<Unit>, int> CreateId(
        IEntityReader reader, IEntityWriter writer, string name, int count, CancellationToken cancellationToken) =>
        Range(0, retryCount)
            .Map(_ => TryCreateId(reader, writer, name, count, cancellationToken))
            .RetryOnConflict()
            .ToAsync()
            .Map(nextId => nextId.Id - count + 1)
            .MapLeft(status => Failure.New(status, $"Cannot create ID for {name}."));

    static EitherAsync<HttpStatusCode, NextId> TryCreateId(
        IEntityReader reader, IEntityWriter writer, string name, int count, CancellationToken cancellationToken) =>
        from tuple in CreateIfNotFound(reader.ReadWithETag<NextId>(name, cancellationToken), name, count)
        from _ in writer.Write(tuple.EntityTracker.GetOperations(), cancellationToken)
        select tuple.NextId;

    static EitherAsync<HttpStatusCode, (EntityTracker EntityTracker, NextId NextId)> CreateIfNotFound(
        EitherAsync<HttpStatusCode, ETaggedEntity<NextId>> either, string name, int count) =>
        either.BiBind(
            entity => UpdateEntity(entity, count),
            status => status switch
            {
                HttpStatusCode.NotFound => CreateNewEntity(name, count),
                _ => LeftAsync<HttpStatusCode, (EntityTracker, NextId)>(status),
            });

    static EitherAsync<HttpStatusCode, (EntityTracker, NextId)> UpdateEntity(ETaggedEntity<NextId> entity, int count) =>
        EitherAsync<HttpStatusCode, (EntityTracker, NextId)>.Right(CreateEntityTrackerForUpdate(entity, UpdateNextId(entity, count)));

    static ETaggedEntity<NextId> UpdateNextId(ETaggedEntity<NextId> entity, int count) =>
        entity with { Value = entity.Value with { Id = entity.Value.Id + count } };

    static (EntityTracker, NextId) CreateEntityTrackerForUpdate(ETaggedEntity<NextId> entity, ETaggedEntity<NextId> updatedEntity) =>
        (new EntityCollector().Add(entity).ToTracker().Update(updatedEntity.Value), updatedEntity.Value);

    static EitherAsync<HttpStatusCode, (EntityTracker, NextId)> CreateNewEntity(string name, int count) =>
        EitherAsync<HttpStatusCode, (EntityTracker, NextId)>.Right(CreateEntityTrackerForAdd(CreateFirstId(name, count)));

    static (EntityTracker, NextId) CreateEntityTrackerForAdd(NextId nextId) =>
        (EntityTracker.CreateEmpty().Add(nextId), nextId);

    static NextId CreateFirstId(string name, int count) =>
        new(name, count);

    static Task<Either<HttpStatusCode, NextId>> RetryOnConflict(this IEnumerable<EitherAsync<HttpStatusCode, NextId>> seq) =>
        seq.Match(
            () => Task.FromResult(Left<HttpStatusCode, NextId>(HttpStatusCode.ServiceUnavailable)),
            async (head, tail) => !await IsConflict(head) ? await head : await RetryOnConflict(tail));

    static Task<bool> IsConflict(EitherAsync<HttpStatusCode, NextId> either) =>
        either.Match(
            _ => false,
            status => status switch
            {
                HttpStatusCode.Conflict => true,
                _ => false,
            });
}
