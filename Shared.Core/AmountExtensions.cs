using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class AmountExtensions
{
    public static Amount Sum(this IEnumerable<Amount> source) =>
        source.Aggregate(Amount.Zero, (sum, value) => sum + value);

    public static Amount Sum<T>(this IEnumerable<T> source, Func<T, Amount> selector) =>
        source.Aggregate(Amount.Zero, (sum, value) => sum + selector(value));
}
