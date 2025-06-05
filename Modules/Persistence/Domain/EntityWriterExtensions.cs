using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System;
using System.Net;
using System.Threading;

namespace Frederikskaj2.Reservations.Persistence;

public static class EntityWriterExtensions
{
    public static EitherAsync<HttpStatusCode, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        this IEntityWriter writer,
        Func<EntityTracker, EntityTracker> track,
        CancellationToken cancellationToken) =>
        writer.Write(collector => collector, track, cancellationToken);

    public static EitherAsync<HttpStatusCode, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        this IEntityWriter writer,
        Func<EntityCollector, EntityCollector> collect,
        Func<EntityTracker, EntityTracker> track,
        CancellationToken cancellationToken) =>
        writer.Write(track(collect(new()).ToTracker()).GetOperations(), cancellationToken);
}
