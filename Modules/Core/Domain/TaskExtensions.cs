using LanguageExt;
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

public static class TaskExtensions
{
    public static EitherAsync<TLeft, TRight> ToRightAsync<TLeft, TRight>(this Task<TRight> task) =>
        task.ToEitherAsync<TLeft, TRight>(Right<TLeft, TRight>);

    public static EitherAsync<TLeft, TRight> ToRightAsync<TLeft, TRight>(this TRight right) =>
        Right<TLeft, TRight>(right).ToAsync();

    static EitherAsync<TLeft, TRight> ToEitherAsync<TLeft, TRight>(this Task<TRight> task, Func<TRight, Either<TLeft, TRight>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<TLeft, TRight>> ToEither(Task<TRight> task, Func<TRight, Either<TLeft, TRight>> f) =>
            f(await task);
    }

    public static EitherAsync<TLeft, TRight2> ToEitherAsync<TLeft, TRight1, TRight2>(this Task<TRight1> task, Func<TRight1, Either<TLeft, TRight2>> f)
    {
        return ToEither(task, f).ToAsync();

        static async Task<Either<TLeft, TRight2>> ToEither(Task<TRight1> task, Func<TRight1, Either<TLeft, TRight2>> f) =>
            f(await task);
    }
}
