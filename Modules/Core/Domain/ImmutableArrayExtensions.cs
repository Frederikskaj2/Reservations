using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Frederikskaj2.Reservations.Core;

public static class ImmutableArrayExtensions
{
    public static ImmutableArray<T> UnsafeNoCopyToImmutableArray<T>(this T[] array) => Unsafe.As<T[], ImmutableArray<T>>(ref array);

    public static T[] UnsafeNoCopyToArray<T>(this ImmutableArray<T> immutableArray) => Unsafe.As<ImmutableArray<T>, T[]>(ref immutableArray);
}
