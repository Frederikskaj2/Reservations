using LanguageExt;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public static class LanguageExtExtensions
{
    public static EitherAsync<HttpStatusCode, Unit> ToEitherAsync(this Task<ResponseMessage> task) =>
        task.ToEitherAsync<HttpStatusCode, ResponseMessage, Unit>(response => response.StatusCode switch
        {
            < (HttpStatusCode) 300 => Prelude.unit,
            _ => response.StatusCode
        });

    public static EitherAsync<L, B> ToEitherAsync<L, A, B>(this Task<A> task, Func<A, Either<L, B>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<L, B>> ToEither(Task<A> task, Func<A, Either<L, B>> f) =>
            f(await task);
    }
}