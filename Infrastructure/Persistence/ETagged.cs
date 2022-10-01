namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

static class ETagged
{
    public static ETagged<T> Create<T>(string id, T item, string eTag) => new(id, item, eTag);
}

record ETagged<T>(string Id, T Item, string ETag);
