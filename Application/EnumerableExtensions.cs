using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class EnumerableExtensions
{
    public static IEnumerable<T> Yield<T>(this T value)
    {
        yield return value;
    }

    public static IEnumerable<T> Filter<T, A>(this IEnumerable<T> source, Option<A> option, Func<T, A, bool> predicate) =>
        option.Case switch
        {
            A value => source.Filter(x => predicate(x, value)),
            _ => source
        };

    public static Amount Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Amount> selector)
    {
        var sum = Amount.Zero;
        foreach (var item in source)
            sum += selector(item);
        return sum;
    }


    public static IEnumerable<(Option<T> Previous, T Current)> AsPrefixPairs<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;
        Option<T> previous = None;
        do
        {
            var current = enumerator.Current;
            yield return (previous, current);
            previous = current;
        } while (enumerator.MoveNext());
    }

    public static IEnumerable<(T Current, Option<T> Next)> AsPostfixPairs<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;
        var current = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var next = enumerator.Current;
            yield return (current, next);
            current = next;
        }
        yield return (current, None);
    }
}
