using Frederikskaj2.Reservations.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client;

public static class PriceExtensions
{
    public static Price Sum(this IEnumerable<Price> source) =>
        source.Aggregate(new Price(), (sum, value) => sum + value);

    public static Price Sum<T>(this IEnumerable<T> source, Func<T, Price> selector) =>
        source.Aggregate(new Price(), (sum, value) => sum + selector(value));
}
