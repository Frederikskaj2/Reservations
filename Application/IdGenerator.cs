using LanguageExt;
using System.Net;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class IdGenerator
{
    const int retryCount = 10;

    public static EitherAsync<Failure, int> CreateId(IPersistenceContextFactory contextFactory, string name, int count = 1) =>
        Range(0, retryCount)
            .ToSeq()
            .Map(_ => TryCreateId(contextFactory, name, count))
            .RetryOnConflict()
            .ToAsync()
            .Map(nextId => nextId.Id - count + 1)
            .MapLeft(status => Failure.New(status, $"Cannot create ID for {name}."));

    static EitherAsync<HttpStatusCode, NextId> TryCreateId(IPersistenceContextFactory contextFactory, string name, int count) =>
        from context in CreateIfNotFound(Read(CreateContext(contextFactory), name), contextFactory, name, count)
        from _ in context.Write()
        select context.Item<NextId>();

    static EitherAsync<HttpStatusCode, IPersistenceContext> Read(IPersistenceContext context, string name) =>
        context.ReadItem<NextId>(name);

    static EitherAsync<HttpStatusCode, IPersistenceContext> CreateIfNotFound(
        EitherAsync<HttpStatusCode, IPersistenceContext> either, IPersistenceContextFactory contextFactory, string name, int count) =>
        either.BiBind(
            context => RightAsync<HttpStatusCode, IPersistenceContext>(CreateNextId(context, count)),
            status => status switch
            {
                HttpStatusCode.NotFound => RightAsync<HttpStatusCode, IPersistenceContext>(CreateFirstId(contextFactory, name, count)),
                _ => LeftAsync<HttpStatusCode, IPersistenceContext>(status)
            });

    static IPersistenceContext CreateFirstId(IPersistenceContextFactory contextFactory, string name, int count) =>
        CreateContext(contextFactory).CreateItem(new NextId(name, count), NextId.GetId);

    static IPersistenceContext CreateNextId(IPersistenceContext context, int count) =>
        context.UpdateItem<NextId>(nextId => nextId with { Id = nextId.Id + count });

    static Task<Either<HttpStatusCode, NextId>> RetryOnConflict(this Seq<EitherAsync<HttpStatusCode, NextId>> seq) =>
        seq.Match(
            () => Task.FromResult(Left<HttpStatusCode, NextId>(HttpStatusCode.ServiceUnavailable)),
            async (head, tail) => await IsNotConflict(head) ? await head : await RetryOnConflict(tail));

    static Task<bool> IsNotConflict(EitherAsync<HttpStatusCode, NextId> either) =>
        either.Match(
            _ => true,
            status => status switch
            {
                HttpStatusCode.Conflict => false,
                _ => true
            });
}
