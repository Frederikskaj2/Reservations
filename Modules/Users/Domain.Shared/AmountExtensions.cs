using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Users;

public static class AmountExtensions
{
    public static Amount Sum(this IEnumerable<Amount> source) =>
        source.Aggregate(Amount.Zero, (sum, value) => sum + value);

    public static Amount Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Amount> selector) =>
        source.Aggregate(Amount.Zero, (current, item) => current + selector(item));
}
