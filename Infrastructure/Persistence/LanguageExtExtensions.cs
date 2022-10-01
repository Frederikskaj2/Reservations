using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

static class LanguageExtExtensions
{
    public static async ValueTask<S> FoldAsync<S, A>(this IEnumerable<A> self, S seed, Func<S, A, ValueTask<S>> f)
    {
        foreach (var item in self)
            seed = await f(seed, item);
        return seed;
    }

}