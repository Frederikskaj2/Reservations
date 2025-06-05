using LanguageExt;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

static class EnumerableExtensions
{
    public static IEnumerable<(Option<T> Previous, T Current)> AsPairs<T>(this IEnumerable<T> source)
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
}
