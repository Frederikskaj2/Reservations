namespace Frederikskaj2.Reservations.Persistence;

public static class QueryFactory
{
    public static IQuery<TDocument> Query<TDocument>() where TDocument : class =>
        new CosmosQuery<TDocument>(
            new(
                Distinct: false,
                Top: null,
                Projection: null,
                Join: null,
                Where: $"""
                        root.d = "{typeof(TDocument).Name}"
                        """,
                OrderBy: null));
}
