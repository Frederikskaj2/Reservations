using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System;
using System.Net;
using System.Threading;

namespace Frederikskaj2.Reservations.Persistence;

public interface IEntityReader
{
    EitherAsync<HttpStatusCode, ETaggedEntity<T>> ReadWithETag<T>(string id, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, ETaggedEntity<T>> ReadWithETag<T>(IIsId id, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, T> Read<T>(string id, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, T> Read<T>(IIsId id, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, OptionalEntity<T>> ReadOptional<T>(string id, Func<T> notFoundFactory, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, OptionalEntity<T>> ReadOptional<T>(IIsId id, Func<T> notFoundFactory, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, Seq<ETaggedEntity<T>>> QueryWithETag<T>(IQuery<T> query, CancellationToken cancellationToken);
    EitherAsync<HttpStatusCode, Seq<T>> Query<T>(IQuery<T> query, CancellationToken cancellationToken);
}
