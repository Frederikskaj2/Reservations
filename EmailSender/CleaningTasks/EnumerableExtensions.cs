using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

static class EnumerableExtensions
{
    public static IEnumerable<IReadOnlyList<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
    {
        if (partitionSize <= 0)
            throw new ArgumentException("Partition size must be positive.", nameof(partitionSize));
        using var enumerator = source.GetEnumerator();
        while (true)
        {
            List<T>? buffer = null;
            while ((buffer is null || buffer.Count < partitionSize) && enumerator.MoveNext())
                (buffer ??= new List<T>(partitionSize)).Add(enumerator.Current);
            if (buffer?.Count > 0)
                yield return buffer;
            else
                yield break;
        }
    }
}
