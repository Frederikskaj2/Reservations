using LanguageExt;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

public static class LanguageExtExtensions
{
    public static async ValueTask<TResult> FoldAsync<TSource, TResult>(
        this IEnumerable<TSource> source, TResult seed, Func<TResult, TSource, ValueTask<TResult>> folder)
    {
        foreach (var item in source)
            seed = await folder(seed, item);
        return seed;
    }

    public static EitherAsync<HttpStatusCode, Unit> ToEitherAsync(this Task<ResponseMessage> task) =>
        task.ToEitherAsync<ResponseMessage, HttpStatusCode, Unit>(
            response => response.StatusCode switch
            {
                < HttpStatusCode.Ambiguous => unit,
                _ => response.StatusCode,
            });

    static EitherAsync<TLeft, TRight> ToEitherAsync<T, TLeft, TRight>(this Task<T> task, Func<T, Either<TLeft, TRight>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<TLeft, TRight>> ToEither(Task<T> task, Func<T, Either<TLeft, TRight>> f) =>
            f(await task);
    }
}
