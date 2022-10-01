using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Frederikskaj2.Reservations.Application;

static class CosmosResults
{
    public static CosmosResults<TItem> Empty<TItem>() => EmptyResults<TItem>.Instance;

    static class EmptyResults<TItem>
    {
        public static readonly CosmosResults<TItem> Instance = new(HttpStatusCode.NoContent, Enumerable.Empty<TItem>());
    }
}

public record CosmosResults<TItem> : CosmosResult
{
    public CosmosResults(HttpStatusCode status) : base(status) => Items = Enumerable.Empty<TItem>();

    public CosmosResults(HttpStatusCode status, IEnumerable<TItem> items) : base(status) => Items = items;

    public IEnumerable<TItem> Items { get; }
}