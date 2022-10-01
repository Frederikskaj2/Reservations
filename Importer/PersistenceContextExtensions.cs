using Frederikskaj2.Reservations.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Importer;

static class PersistenceContextExtensions
{
    public static IPersistenceContext CreateItems<T>(this IPersistenceContext context, IEnumerable<T> items, Func<T, string> idFunc) where T : class =>
        items.Aggregate(context, (current, item) => current.CreateItem(item, idFunc));
}
