using LanguageExt;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

static class EnumerableExtensions
{
    public static IEnumerable<(T Current, Option<T> Next)> AsPairs<T>(this IEnumerable<T> source)
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
