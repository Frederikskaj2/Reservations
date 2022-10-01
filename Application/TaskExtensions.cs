using LanguageExt;
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class TaskExtensions
{
    public static EitherAsync<L, R> ToRightAsync<L, R>(this Task<R> task) =>
        task.ToEitherAsync<L, R>(Right<L, R>);

    public static EitherAsync<L, A> ToEitherAsync<L, A>(this Task<A> task, Func<A, Either<L, A>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<L, A>> ToEither(Task<A> task, Func<A, Either<L, A>> f) =>
            f(await task);
    }

    public static EitherAsync<L, B> ToEitherAsync<L, A, B>(this Task<A> task, Func<A, Either<L, B>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<L, B>> ToEither(Task<A> task, Func<A, Either<L, B>> f) =>
            f(await task);
    }
}