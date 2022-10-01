using Frederikskaj2.Reservations.Application;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

public class Untracked : IUntracked
{

    internal Untracked(CosmosContext context) => Context = context;

    CosmosContext Context { get; }

    public EitherAsync<HttpStatusCode, T> ReadItem<T>(string id) where T : class =>
        GetItem(Context.Cosmos.ReadAsync<T>(id, Context.PartitionKey));

    public EitherAsync<HttpStatusCode, T> ReadItem<T>(string id, Func<T> notFoundFactory) =>
        GetOrCreate(Context.Cosmos.ReadAsync<T>(id, Context.PartitionKey), notFoundFactory);

    static EitherAsync<HttpStatusCode, T> GetOrCreate<T>(ValueTask<ETaggedResult<T>> task, Func<T> notFoundFactory)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, T>> ToEither()
        {
            var (status, eTagged) = await task;
            return status switch
            {
                HttpStatusCode.NotFound => notFoundFactory(),
                < (HttpStatusCode) 300 => eTagged.ValueUnsafe().Item,
                _ => status
            };
        }
    }

    public EitherAsync<HttpStatusCode, IEnumerable<T>> ReadItems<T>(IQuery<T> query) =>
        GetItems(Context.Cosmos.QueryAsync<T>(query.Sql, Context.PartitionKey));

    public EitherAsync<HttpStatusCode, IEnumerable<T>> ReadItems<T>(IProjectedQuery<T> query) =>
        GetItems(Context.Cosmos.QueryProjectedAsync<T>(query.Sql, Context.PartitionKey));

    static EitherAsync<HttpStatusCode, T> GetItem<T>(ValueTask<ETaggedResult<T>> task)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, T>> ToEither()
        {
            var (status, eTagged) = await task;
            return status switch
            {
                < (HttpStatusCode) 300 => eTagged.ValueUnsafe().Item,
                _ => status
            };
        }
    }

    static EitherAsync<HttpStatusCode, IEnumerable<T>> GetItems<T>(ValueTask<ETaggedResults<T>> task)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IEnumerable<T>>> ToEither()
        {
            var (status, items) = await task;
            return status switch
            {
                < (HttpStatusCode) 300 => Right(items.Map(eTagged => eTagged.Item)),
                _ => status
            };
        }
    }

    static EitherAsync<HttpStatusCode, IEnumerable<T>> GetItems<T>(ValueTask<Results<T>> task)
    {
        return ToEither().ToAsync();

        async Task<Either<HttpStatusCode, IEnumerable<T>>> ToEither()
        {
            var (status, items) = await task;
            return status switch
            {
                < (HttpStatusCode) 300 => Right(items),
                _ => status
            };
        }
    }
}
